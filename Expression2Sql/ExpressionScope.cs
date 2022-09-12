using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.Loader;
using QueryingCore.AggregativeIndicatorCore;
using QueryingCore.CodeGeneration;
using QueryingCore.Core;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using RestApi.Data.Models;

namespace QueryingCore;

public sealed class ExpressionScope : IDisposable
{
    readonly Lazy<AssemblyLoadContext> _assemblyLoadContext;

    const string ExpressionWrapper = nameof(ExpressionWrapper);
    const string FilterWrapper = nameof(FilterWrapper);

    readonly DataManager _dataManager;

    readonly ProjectModel _project;

    readonly CodeBuilder _codeBuilder;

    FactoryBase? _factoryBase;
    internal FactoryBase Factory => _factoryBase ?? throw new InvalidOperationException("NotInitialized");

    static int _assemblyVersion = 0;

    public Guid SourceId { get; private set; }

    public Guid? SourcePairId { get; private set; }

    public Guid? AnotherSourceId { get; private set; }

    public List<string> Errors { get; private set; } = new List<string>();

    public ILogger Logger { get; }

    readonly Corrector _corrector;

    public CodeGenerator CodeGenerator { get; }

    public Type? ExpressionWrapperType { get; set; }

    public Type? FilterWrapperType { get; set; }

    ExpressionScope(ProjectModel project, ILogger? logger)
    {
        _corrector = new Corrector();
        _project = project;
        Logger = logger ?? NullLogger<ExpressionScope>.Instance;
        var dataManager = new DataManager(_corrector, _project);
        _codeBuilder = new CodeBuilderFactory(_corrector).Create(_project);
        CodeGenerator = new CodeGenerator(dataManager, _project, _codeBuilder);
        _dataManager = dataManager;
        _assemblyLoadContext = new Lazy<AssemblyLoadContext>(() => new AssemblyLoadContext(default, true));
    }

    public static ExpressionScope Create(ProjectModel project, Guid sourceId, Guid? sourcePairId, string? expression, string? filter = null, ILogger? logger = default)
    {
        ExpressionScope expressionScope = new (project, logger) { SourceId = sourceId, SourcePairId = sourcePairId, AnotherSourceId = project.GetAnotherSourceId(sourceId, sourcePairId) };

        var expressionLambdaText = expressionScope.ConvertedToLambda(expression, sourceId, sourcePairId);
        var filterLambdaText = string.IsNullOrWhiteSpace(filter) ? null : expressionScope.ConvertedToLambda(filter, expressionScope.AnotherSourceId.Value, sourcePairId);

        if (expressionScope.Errors.Any())
            return expressionScope;

        var (code, dateConstants) = expressionScope.CodeGenerator.Generate();

        var assembly = expressionScope.GenerateAssembly(code, dateConstants, expressionLambdaText, filterLambdaText, expressionScope._project.Sources[sourceId].Tag,
            !string.IsNullOrWhiteSpace(filter)
                ? expressionScope._project.Sources[expressionScope.AnotherSourceId.Value].Tag
                : string.Empty);

        if (expressionScope.Errors.Any()) return expressionScope;

        Type type = assembly.GetTypes().FirstOrDefault(x => x.IsSubclassOf(typeof(FactoryBase)));
        expressionScope._factoryBase = (FactoryBase) Activator.CreateInstance(type);

        expressionScope.CreateDbContext();

        return expressionScope;
    }
    
    public static ExpressionScope CreateIndicatorScope(ProjectModel project, Guid indicatorId, ILogger? logger = default)
    {
        var indicator = project.Indicators[indicatorId];

        var expressionScope = new ExpressionScope(project, logger) { SourceId = indicator.SourceId!.Value, SourcePairId = indicator.SourcePairId };

        List<(Guid, string)> rulesLambdaText = new();

        foreach (var (guid, rule) in indicator.Rules)
        {
            var expression = expressionScope.ConvertedToLambda<bool>(rule.Expression, indicator.SourceId.Value, indicator.SourcePairId);            
        }

        var (code, dateConstants) = expressionScope.CodeGenerator.Generate();

        var assembly = expressionScope.GenerateAssembly(code, dateConstants,
            expressionScope._project.Sources[indicator.SourceId.Value].Tag, rulesLambdaText);

        if (expressionScope.Errors.Any())
            return expressionScope;

        Type type = assembly.GetTypes().FirstOrDefault(x => x.IsSubclassOf(typeof(FactoryBase)));
        expressionScope._factoryBase = (FactoryBase) Activator.CreateInstance(type);

        expressionScope.CreateDbContext();

        return expressionScope;
    }

    string ConvertedToLambda(string? lambda, Guid sourceId, Guid? sourcePairId)
    {
        string? convertedToLambda = lambda;
        if (!string.IsNullOrEmpty(lambda))
        {
            var simple = new SimpleSyntaxScope.SimpleSyntaxScope(_dataManager, _codeBuilder, sourceId, sourcePairId);

            if (!lambda.Contains("=>"))
            {
                IEnumerable<string> simpleErrors;

                (convertedToLambda, simpleErrors) = simple.ConvertToLambdaSyntax(lambda);

                if (simpleErrors.Any())
                {
                    Errors.AddRange(simpleErrors);
                }
            }
        }
        else
        {
            convertedToLambda = "x => null";
        }

        return convertedToLambda;
    }

    string ConvertedToLambda<T>(string? lambda, Guid sourceId, Guid? sourcePairId)
    {
        string? convertedToLambda = lambda;
        if (!string.IsNullOrEmpty(lambda))
        {
            var simple = new SimpleSyntaxScope.SimpleSyntaxScope(_dataManager, _codeBuilder, sourceId, sourcePairId);

            if (!lambda.Contains("=>"))
            {
                IEnumerable<string> simpleErrors;

                (convertedToLambda, simpleErrors) = simple.ConvertToLambdaSyntax<T>(lambda);

                if (simpleErrors.Any())
                {
                    Errors.AddRange(simpleErrors);
                }
            }
        }
        else
        {
            convertedToLambda = $"x => ({CodeGenerator.GetTypeName<T>()})null";
        }

        return convertedToLambda;
    }

    PortableExecutableReference[] GetReferences()
    {
        string dllString = AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES").ToString();
        var dlls = dllString.Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries).ToList();

        dlls.Add(typeof(DbContext).Assembly.Location);
        dlls.Add(typeof(RelationalEntityTypeBuilderExtensions).Assembly.Location);
        dlls.Add(typeof(IndicatorResult).Assembly.Location);
        PortableExecutableReference[] references = dlls.Select(x => MetadataReference.CreateFromFile(x)).ToArray();
        return references;
    }

    Assembly? GenerateAssembly(string code, string dateConstants, string? expression, string? filter,
        string sourceTag, string anotherSourceTag)
    {
        var assemblyName = AddNamespace(code, out string codeToCompile);

        if (!string.IsNullOrEmpty(expression))
            codeToCompile +=
                $"public class {ExpressionWrapper} {{ {dateConstants} public Expression<Func<{CodeGenerator.GetRecordClassName(sourceTag)}, object>> expr => {expression}; }}{Environment.NewLine}";
        if (!string.IsNullOrEmpty(filter))
            codeToCompile +=
                $"public class {FilterWrapper} {{ {dateConstants} public Expression<Func<{CodeGenerator.GetRecordClassName(anotherSourceTag)}, object>> expr => {filter}; }}";

        codeToCompile += $"{Environment.NewLine}}}";

        return CompileAssembly(codeToCompile, assemblyName);
    }
    
    Assembly? GenerateAssembly(string code, string dateConstants, string sourceTag, IEnumerable<(Guid, string)> rulesLambdaText)
    {
        var assemblyName = AddNamespace(code, out string codeToCompile);

        codeToCompile += $"public class {ExpressionWrapper}{Environment.NewLine}{{{Environment.NewLine}{dateConstants}";
        foreach (var (guid, expression) in rulesLambdaText)
        {
            codeToCompile +=
                $" public Expression<Func<{CodeGenerator.GetRecordClassName(sourceTag)}, bool>> _{guid:N} => {expression};{Environment.NewLine}";            
        }

        codeToCompile += $"{Environment.NewLine}}}{Environment.NewLine}";
        

        codeToCompile += $"{Environment.NewLine}}}";

        return CompileAssembly(codeToCompile, assemblyName);
    }

    Assembly? CompileAssembly(string codeToCompile, string assemblyName)
    {
        var parseOptions = CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.CSharp8);
        var parsedSyntaxTree = SyntaxFactory.ParseSyntaxTree(codeToCompile, parseOptions);

        var compilationOptions = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
            .WithOverflowChecks(false)
            .WithOptimizationLevel(OptimizationLevel.Release);

        var references = GetReferences();

        var compilation = CSharpCompilation.Create(assemblyName, new[] {parsedSyntaxTree}, references, compilationOptions);

        using var ms = new MemoryStream();

        var result = compilation.Emit(ms);
        if (!result.Success)
        {
            Errors.AddRange(result.Diagnostics.Where(x => x.Severity == DiagnosticSeverity.Error).Select(x => x.ToString()).ToList());

            return default;
        }

        ms.Seek(0, SeekOrigin.Begin);
        var assembly = _assemblyLoadContext.Value.LoadFromStream(ms);

        ExpressionWrapperType = assembly.GetType($"{assemblyName}.{ExpressionWrapper}");
        FilterWrapperType = assembly.GetType($"{assemblyName}.{FilterWrapper}");

        return assembly;
    }

    string AddNamespace(string code, out string codeToCompile)
    {
        var assemblyName = $"AggregativeIndicator.p{_dataManager.TheVersion:n}.v{_assemblyVersion}";

        codeToCompile = $"namespace {assemblyName}{Environment.NewLine}{{{Environment.NewLine}{code}{Environment.NewLine}";

        return assemblyName;
    }

    void CreateDbContext()
    {
        const string? fakeConnectionString = "Host=_;Database=_;Username=_;Password=_);";
        var options = new DbContextOptionsBuilder()
            .UseNpgsql(fakeConnectionString)
            .Options;

        var dbContextClassName = CodeGenerator.GetDbContextClassName(_dataManager.TheVersion);
        var type = Factory.GetType();
        Type dbContextType = type.Assembly.GetType($"{type.Namespace}.{dbContextClassName}");
        Ctx = (DbContext) Activator.CreateInstance(dbContextType, (object) options);
    }

    //todo factory should be initialized
    public UserFieldScope GetUserFieldScope(Guid sourceId, Guid sourcePairId) => new(this, _factoryBase, sourceId, _project.GetAnotherSourceId(sourceId, sourcePairId), sourcePairId, _project, _dataManager.TheVersion);
    
    public IEnumerable<(Guid, string, string error)> ValidateRule()
    {
        var ruleErrors = new List<(Guid ruleId, string ruleExpression, string error)>();

        (Expression? exp, string error) = CovertToLambda(SourceId, SourcePairId!.Value);

        if (!string.IsNullOrEmpty(error))
        {
            ruleErrors.Add((default, exp.ToString(), error));
            return ruleErrors;
        }

        //Проверка генерируемости SQL
        try
        {
            _ = CreateQuery(Ctx, new[] {(Guid.Empty, exp)}).Take(1).ToQueryString();
        }
        catch (InvalidOperationException ex) when (ex.Source == "Microsoft.EntityFrameworkCore")
        {
            error = ex.Message;
            ruleErrors.Add((default, exp.ToString(), error));
        }
        return ruleErrors;
    }

    public IEnumerable<(Guid, string, string error)> ValidateDateRule(Guid? ruleId)
    {
        var ruleErrors = new List<(Guid ruleId, string ruleExpression, string error)>();

        var res = CovertToLambda(SourceId, SourcePairId!.Value, ruleId);        

        //Проверка генерируемости SQL
        /*try
        {
            _ = CreateQuery(Ctx, new[] { (Guid.Empty, res.expression, date.expression) }).Take(1).ToQueryString();
        }
        catch (InvalidOperationException ex) when (ex.Source == "Microsoft.EntityFrameworkCore")
        {
#if DEBUG
            ruleErrors.Add((default, date.expression.ToString(), ex.ToString()));
#else
            ruleErrors.Add((default, date.expression.ToString(), ex.Message));
#endif
        }
        catch (InvalidCastException ex)
        {
#if DEBUG
            ruleErrors.Add((default, date.expression.ToString(), ex.ToString()));
#else
            ruleErrors.Add((default, date.expression.ToString(), "Ошибка компиляции выражения. Ожидаемый результат должен иметь тип bool."));
#endif

        }*/

        return ruleErrors;
    }
        
    public (Expression? expression, string error) CovertToLambda(Guid sourceId, Guid? sourcePairId, Guid? ruleId = null)
    {
        try
        {
            Expression exp;
            if (ruleId is null)
            {
                IQueryingContext expressionContext;
                if (sourcePairId.HasValue)
                    expressionContext = Factory.GetContext((sourceId, sourcePairId.Value));
                else
                    expressionContext = Factory.GetContext(sourceId);

                exp = expressionContext.Evaluate(ExpressionWrapperType!);
            }
            else
                exp = Factory.EvaluateRule(sourceId, ExpressionWrapperType!, ruleId.Value);

            return (exp, string.Empty);
        }
        catch (ArgumentException ex)
        {
            return (default, ex.Message);
        }
        catch (CompilationErrorException ex)
        {
            return (default, ex.Message);
        }
    }
    
    public (Expression? expression, string error) CovertFilterToLambda(Guid sourceId, Guid? sourcePairId)
    {
        try
        {
            Expression exp;
            
            IQueryingContext expressionContext;
            if (sourcePairId.HasValue)
                expressionContext = Factory.GetContext((sourceId, sourcePairId.Value));
            else
                expressionContext = Factory.GetContext(sourceId);

            exp = expressionContext.Evaluate(FilterWrapperType!);
            

            return (exp, string.Empty);
        }
        catch (ArgumentException ex)
        {
            return (default, ex.Message);
        }
        catch (CompilationErrorException ex)
        {
            return (default, ex.Message);
        }
    }

    public IQueryable<IndicatorResult> CreateQuery(DbContext ctx, IEnumerable<(Guid ruleId, Expression ruleExpression)> rules)
    {
        var rulesList = new List<(Guid ruleId, Expression ruleExpression)>();
        foreach (var (ruleId, ruleExpression) in rules)
            rulesList.Add((ruleId, new ConstantsResolver().Visit(ruleExpression)));

        return Factory.GetQuery(ctx, SourceId, rulesList);
    }

    public DbContext? Ctx { get; private set; }

    public void Dispose()
    {
        Ctx?.Dispose();
        if (_assemblyLoadContext.IsValueCreated)
            _assemblyLoadContext.Value.Unload();

        GC.Collect();
    }
}
