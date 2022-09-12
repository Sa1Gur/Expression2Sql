using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using QueryingCore.Core;
using RestApi.Data.Models;

namespace QueryingCore.CodeGeneration;

public sealed class CodeGenerator
{
	public const string CalculatedAt = nameof(CalculatedAt);

    public const string ActualAt = nameof(ActualAt);

    int _callCount;
    readonly StringBuilder _sb = new();
    readonly DataManager _dataManager;
    readonly ProjectModel _project;
    StringBuilder _dateConstants;

    readonly CodeBuilder _codeBuilder;

    public CodeGenerator(DataManager dataManager, ProjectModel project, CodeBuilder codeBuilder)
    {
        _dataManager = dataManager;
        _dataManager.CodeGenerator = this;
        _project = project;
        _codeBuilder = codeBuilder;
    }

    public (string, string) Generate()
    {
        if (Interlocked.Increment(ref _callCount) != 1)
            throw new InvalidOperationException($"Only one call of {nameof(Generate)} expected");

        _sb.AppendLine(string.Join(Environment.NewLine,
            new[]
            {
                "System",
                "System.Data",
                "System.Linq",
                "System.Linq.Expressions",
                "System.Threading.Tasks",
                "System.Collections.Concurrent",
                "System.Collections.Generic",
                "Microsoft.CodeAnalysis.CSharp.Scripting",
                "Microsoft.EntityFrameworkCore",
                "Microsoft.EntityFrameworkCore.Storage",
                "Microsoft.EntityFrameworkCore.Metadata.Builders",
                "Microsoft.EntityFrameworkCore.Query.SqlExpressions",
                "QueryingCore.Core",
                "QueryingCore.AggregativeIndicatorCore"
            }.Select(x => $"\tusing {x};")));//todo third place to add namespaces
        _sb.AppendLine();
        
        _sb.AppendLine("\tusing static QueryingCore.Core.ModelBuilderExtensions;");
        _sb.AppendLine("\tusing static QueryingCore.Core.CustomFunctionExtensions;");
        _sb.AppendLine("\tusing static Project;");
        _sb.AppendLine();

        AppendProjectClass();

        AppendDbContextClass();

        // Формирование классов Record и RecordConfiguration по "Источнику"
        foreach (var (_, sourceTag, sourceId) in _dataManager.GetSources())
        {
            // Формирование класса Record
            AppendRecordClass(sourceTag, sourceId);

            // Формирование класа RecordConfiguration
            AppendRecordClassConfiguration(sourceTag, sourceId);
        }

        return (_sb.ToString(), _dateConstants.ToString());
    }

    public void AppendRecordClass(string sourceTag, Guid sourceId)
    {
        _sb.AppendLine($"\t[SourceId(\"{sourceId}\")]");
        _sb.AppendLine($"\tpublic class {sourceTag}Record : RecordBase");
        _sb.AppendLine("\t{");

        AddMainSourceAttributes(sourceId, sourceTag);

        foreach (var otherSourceId in _dataManager.GetSourceLinkedItems(
            sourceId)
            .GroupBy(x => x.otherSourceId)
            .Select(group => group.First().otherSourceId))//todo solve it one way or ПИХНУТь в него
        {
            var (_, otherSourceTag, _) = _dataManager.GetSources().First(x => x.id == otherSourceId);
            
            AddOtherSourceAttributesWithTag(otherSourceId, otherSourceTag);

            AddUserFields(otherSourceId, otherSourceTag, false);
        }

        foreach (var (linkedItemsPropertyName, sourcePairId, otherSourceId) in _dataManager.GetSourceLinkedItems(
            sourceId)
            .GroupBy(x => x.linkedItemsPropertyName)//todo solve it one way or another (ambiguity about links between the same sources)
            .Select(group => group.First()))
        {
            var (_, otherSourceTag, _) = _dataManager.GetSources().First(x => x.id == otherSourceId);
            _sb.AppendLine($"\t[SourcePairId(\"{sourcePairId}\")]");
            _sb.AppendLine($"\t[RuleDependency(\"{sourcePairId}\")]");
            _sb.AppendLine(
                $"\tpublic virtual ICollection<{GetRecordClassName(otherSourceTag)}> {linkedItemsPropertyName} {{ get; set; }}");
        }

        // формирование списка индикаторов
        foreach (var (_, name, indicatorId) in _dataManager.GetSourceIndicators(sourceId))
        {
            _sb.AppendLine($"\t[SourceIndicator(\"{indicatorId}\")]");
            _sb.AppendLine($"\t[RuleDependency(\"{indicatorId}\")]");
            _sb.AppendLine($"\tpublic {Types.GetNetTypeString(typeof(Guid?))} {name} {{ get; set; }}");
        }

        AddUserFields(sourceId, sourceTag);

        _sb.Append(_codeBuilder.GetSource(sourceId).AppendRecordClassContent, 2);

        // закрывающая скобка класса
        _sb.AppendLine("\t}");
        _sb.AppendLine();
    }

    void AddUserFields(Guid sourceId, string sourceTag, bool allowTagless = true)
    {
        var userFields =
            _project.UserFields.Where(x => x.Value.SourceId == sourceId && x.Value.Type != DataTypes.Unknown).ToList();

        if (!userFields.Any()) return;
        
        _sb.AppendLine($"\t// User fields ({userFields.Count})");
        foreach (var (guid, userFieldModel) in userFields)
        {
            var propertyType = Types.GetNetType(userFieldModel.Type);
            if (propertyType == typeof(Types.Undefined)) continue;

            string propertyTypeString = Types.GetNetTypeString(propertyType);
            string propertyName = GetRecordPropertyName(userFieldModel);

            if (allowTagless)
            {
                _sb.AppendLine($"\t[RuleDependency(\"{guid}\")]");
                _sb.AppendLine($"\tpublic {propertyTypeString} {propertyName} {{ get; set; }}");
            }
                
            _sb.AppendLine($"[RuleDependency(\"{guid}\")]");
            _sb.AppendLine($"\tpublic {propertyTypeString} {sourceTag}_{propertyName} {{ get; set; }}");
        }
    }

    void AddMainSourceAttributes(Guid sourceId, string sourceTag)
    {
        foreach (var (_, name, type, _) in _dataManager.GetSourceAttributes(sourceId))
        {
            _sb.AppendLine($"\tpublic {Types.GetNetTypeString(type)} {name} {{ get; set; }}");

            _sb.AppendLine($"\tpublic {Types.GetNetTypeString(type)} {sourceTag}_{name} {{ get; set; }}");
        }
    }

    void AddOtherSourceAttributesWithTag(Guid otherSourceId, string otherSourceTag)
    {
        foreach (var (_, name, type, _) in _dataManager.GetSourceAttributes(otherSourceId))
        {
            _sb.AppendLine($"\tpublic {Types.GetNetTypeString(type)} {otherSourceTag}_{name} {{ get; set; }}");
        }
    }

    public void AppendRecordClassConfiguration(string sourceTag, Guid sourceId)
    {
        _sb.AppendLine(
            $"internal sealed class {GetRecordConfigurationClassName(sourceTag)} : IEntityTypeConfiguration<{GetRecordClassName(sourceTag)}>");
        //открывающая класса
        _sb.AppendLine("{");

        _sb.AppendLine("private readonly string _tableName;");
        _sb.AppendLine($"public {GetRecordConfigurationClassName(sourceTag)}(string tableName)");
        _sb.AppendLine("{");
        _sb.AppendLine("_tableName = tableName;");

        _sb.AppendLine("}");

        //добавляем метод
        _sb.AppendLine($"\tpublic void Configure(EntityTypeBuilder<{GetRecordClassName(sourceTag)}> builder)");
        _sb.AppendLine("\t{");
        _sb.AppendLine($"\t\tbuilder.ToTable(_tableName, \"fake_table_name\");");
        _sb.AppendLine($"\t\tbuilder.HasKey(x => x.Id);");
        _sb.AppendLine($"\t\tbuilder.Property(x => x.Id).HasColumnName(\"id\");");

        foreach (var (_, propertyName, _, columnName) in _dataManager.GetSourceAttributes(sourceId))
        {
            _sb.AppendLine($"\t\tbuilder.Property(x => x.{propertyName}).HasColumnName(\"{columnName}\");");
        }

        foreach (var (linkedItemsPropertyName, _, _) in _dataManager.GetSourceLinkedItems(sourceId)
            .GroupBy(x => x.linkedItemsPropertyName)
            .Select(y => y.First()))//todo solve it one way or ПИХНУТь в него
        {
            _sb.AppendLine($"\t\tbuilder.Ignore(x => x.{linkedItemsPropertyName});");
        }

        foreach (var (_, name, _) in _dataManager.GetSourceIndicators(sourceId))
        {
            _sb.AppendLine($"\t\tbuilder.Ignore(x => x.{name});");
        }

        var userFields =
            _project.UserFields.Where(x => x.Value.SourceId == sourceId && x.Value.Type != DataTypes.Unknown);
        if (userFields.Any())
        {
            _sb.AppendLine($"// Append user fields configuration ({userFields.Count()})");
            foreach (var userField in userFields)
            {
                var propertyName = GetRecordPropertyName(userField.Value);
                var columnName = userField.Value.ColumnName;
                _sb.AppendLine($"\t\tbuilder.Property(x => x.{propertyName}).HasColumnName(\"{columnName}\");");
            }
        }

        _sb.Append(_codeBuilder.GetSource(sourceId).AppendConfigureContent, 3);

        //закрывающая от метода
        _sb.AppendLine("\t}");

        //закрывающая от класса
        _sb.AppendLine("}");
        _sb.AppendLine();
    }

    //Project - статический класс, который должен содержать все константы проекта.
    void AppendProjectClass()
    {
        _sb.AppendLine($"\tinternal class Factory: {nameof(FactoryBase)}");
        _sb.AppendLine("\t{");

        //конструктор
        _sb.AppendLine($"\t\tpublic Factory()");
        _sb.AppendLine("\t\t{");

        foreach (var (_, sourceTag, _) in _dataManager.GetSources())
        {
            _sb.AppendLine(
                $"\t\t\tRuleTypes.TryAdd({GetProjectClassName()}.{GetStaticClassName(sourceTag)}SourceId, typeof(Expression<Func<{GetRecordClassName(sourceTag)}, bool>>));");
            _sb.AppendLine(
                $"\t\t\tRuleEvaluators.TryAdd({GetProjectClassName()}.{GetStaticClassName(sourceTag)}SourceId, (typeWrapper, ruleId) => QueryingContext<{GetRecordClassName(sourceTag)}>.CSharpScriptEvaluate(typeWrapper, ruleId));");
            _sb.AppendLine(
                $"\t\t\tIndicatorQueryBuilders.TryAdd({GetProjectClassName()}.{GetStaticClassName(sourceTag)}SourceId,{GetQueryBuilderFunctionName(sourceTag)});");
        }

        foreach (var scope in GetQueryingContexts())
        {
            _sb.AppendLine($"\t\t\tAddContext(new Guid(\"{scope.Key}\"), {GetQueryingContextConstructorCall(scope)});");
        }

        foreach (var scope in GetCreateUserFieldExpressionMethodsScopeList())
        {
            _sb.AppendLine($"\t\t\tAddContext((new Guid(\"{scope.source.Key}\"), new Guid(\"{scope.sourcePair.Key}\")), {GetCreateUserFieldConstructorCall(scope)});");
        }

        //закр конструктора
        _sb.AppendLine("\t\t}");
        _sb.AppendLine();

        foreach (var (_, sourceName, sourceId) in _dataManager.GetSources())
        {
            AppendReplaceSurrogates(sourceName, sourceId);
            AppendQueryBuilder(sourceName);
        }

        foreach (var scope in GetCreateUserFieldExpressionMethodsScopeList())
        {
            AppendCreateUserFieldExpressionMethod(scope);
        }

        _sb.AppendLine("}");

        _sb.Append(_codeBuilder.AppendClasses, 0);

        //добавление списков правил
        foreach (var (_, _, sourceId) in _dataManager.GetSources())
        {
            foreach (var (_, indicatorName, indicatorId) in _dataManager.GetSourceIndicators(sourceId))
            {
                if (_codeBuilder.FindSourcePairIndicator(indicatorName) != default) continue;

                _sb.AppendLine($"\t\tpublic static class {indicatorName}Rules //rules");
                //открывающая класса
                _sb.AppendLine("\t\t{");
                foreach (var (_, ruleName, ruleId) in _dataManager.GetSourceIndicatorRules(indicatorId))
                {
                    _sb.AppendLine(
                        $"\tpublic static readonly {Types.GetNetTypeString(typeof(Guid))} {ruleName} = Guid.Parse(\"{ruleId}\"); //rule");
                }

                //закрывающая класса
                _sb.AppendLine("\t\t}");
            }
        }

        _sb.AppendLine($"public class {GetProjectClassName()} //project");
        //открывающая класса
        _sb.AppendLine("{");

        //константы проекта
        foreach (var (_, name, type, value) in _dataManager.GetConstants())
        {
            if (!_map.ContainsKey(type))
            {
                throw new InvalidOperationException("CodeGeneration");
            }

            _sb.AppendLine($"public static readonly {Types.GetNetTypeString(type)} {name} = {_map[type](value)};");
        }
        
        //идентификаторы индикаторов 
        foreach (var (_, sourceName, sourceId) in _dataManager.GetSources())
        {
            foreach (var (_, indicatorName, indicatorId) in _dataManager.GetSourceIndicators(sourceId))
            {
                if (_codeBuilder.FindSourcePairIndicator(indicatorName) != default) continue;
                AppendIndicator(sourceName, indicatorName, indicatorId);
            }
        }

        HashSet<string> doneLinks = new HashSet<string>();

        foreach (var (_, sourceName, sourceId) in _dataManager.GetSources())
        {
            if (doneLinks.Add(sourceName))
            {
                _sb.AppendLine(
                    $"\tpublic static readonly {Types.GetNetTypeString(typeof(Guid))} {sourceName}SourceId = Guid.Parse(\"{sourceId}\");");
            }

            foreach (var (linkedItemsPropertyName, linkId, anotherSourceId) in _dataManager.GetSourceLinkedItems(
                sourceId)
                .GroupBy(x => x.linkedItemsPropertyName)
                .Select(y => y.First()))//todo solve it one way or another (ambiguity about links between the same sources
            {
                var (_, anotherSourceName, _) =
                    _dataManager.GetSources().FirstOrDefault(x => x.id == anotherSourceId);

                if (doneLinks.Add(linkedItemsPropertyName))
                {
                    _sb.AppendLine(
                        $"public static readonly {Types.GetNetTypeString(typeof(Guid))} {linkedItemsPropertyName}Id = Guid.Parse(\"{linkId}\");");
                }

                _sb.AppendLine(
                    $"\tpublic static Expression<Func<{GetRecordClassName(sourceName)}, IEnumerable<{GetRecordClassName(anotherSourceName)}>>> " +
                    $"Get{sourceName}_{linkedItemsPropertyName}Subquery(DbContext ctx)");

                _sb.AppendLine("\t{");

                // формируем oneInPair
                _sb.AppendLine($"var oneInPair = ctx.Set<{GetPairConnectionRecordClassName()}>().Where" +
                               $"(x => x.SourcePairId == {linkedItemsPropertyName}Id && x.SourceId == {GetStaticClassName(sourceName)}SourceId);");

                // формируем anotherInPair
                _sb.AppendLine($"var anotherInPair = ctx.Set<{GetPairConnectionRecordClassName()}>().Where" +
                               $"(x => x.SourcePairId == {linkedItemsPropertyName}Id && x.SourceId == {GetStaticClassName(anotherSourceName)}SourceId);");

                // связываем oneInPair с anotherInPair
                _sb.Append(
                    "var p = oneInPair.Join(anotherInPair, sp2 => sp2.Link, sp3 => sp3.Link, (sp2, sp3) => new { sp2.RowId, extId = sp3.RowId })");
                // добавляем данные из another
                _sb.AppendLine(
                    $".Join(ctx.Set<{GetRecordClassName(anotherSourceName)}>(), p => p.extId, p => p.Id, (s, ext) => new {{ s.RowId, ext }});");

                //возвращаем 
                _sb.AppendLine("return x => p.Where(z => z.RowId == x.Id).Select(x => x.ext);");

                //закрывающая метода
                _sb.AppendLine("\t}");
            }
        }

        _sb.Append(_codeBuilder.AppendProjectClassContent, 1);

        //закрывающая класса
        _sb.AppendLine("}");
    }

    IEnumerable<KeyValuePair<Guid, SourceModel>> GetQueryingContexts()
    {
        foreach (var source in _project.Sources)
        {
            yield return source;
        }
    }

    string GetQueryingContextConstructorCall(KeyValuePair<Guid, SourceModel> source) =>
        $"new QueryingContext<{GetRecordClassName(_dataManager.Corrector.Correct(source.Value.Tag))}>()";

    //Список функций CreateUserFieldExpressionDelegate
    IEnumerable<(KeyValuePair<Guid, SourceModel> source, KeyValuePair<Guid, SourcePairModel> sourcePair)>
        GetCreateUserFieldExpressionMethodsScopeList()
    {
        // для каждой пары {sourceId, sourcePairId} нужен свой метод и реализация dispatcher
        foreach (var source in _project.Sources)
        {
            foreach (var sourcePair in _project.SourcePairs.Where(x =>
                x.Value.Items.Any(x => x.Value.SourceId == source.Key)))
                yield return (source, sourcePair);
        }
    }

    string GetCreateUserFieldExpressionMethodName((
        KeyValuePair<Guid, SourceModel> source,
        KeyValuePair<Guid, SourcePairModel> sourcePair) scope)
        => $"createUserFieldExpression_{scope.source.Key:n}_{scope.sourcePair.Key:n}";

    string GetCreateUserFieldConstructorCall((
        KeyValuePair<Guid, SourceModel> source,
        KeyValuePair<Guid, SourcePairModel> sourcePair) scope)
    {
        string sourceRecordType = GetRecordClassName(_dataManager.Corrector.Correct(scope.source.Value.Tag));
        var anotherSourceId = _project.GetAnotherSourceId(scope.source.Key, scope.sourcePair.Key);
        var anotherSource = _project.GetSourceOrDefault(anotherSourceId);
        string anotherSourceRecordType = GetRecordClassName(_dataManager.Corrector.Correct(anotherSource.Tag));

        string linkItemsPropertyName = GetLinkedItemsPropertyName(scope.sourcePair.Value);

        var s = $"Get{GetStaticClassName(_dataManager.Corrector.Correct(scope.source.Value.Tag))}_{linkItemsPropertyName}Subquery";

        return
            $"new QueryingContextAggregative<{sourceRecordType},{anotherSourceRecordType}>(x => x.{linkItemsPropertyName}, Project.{s})";
    }

    void AppendCreateUserFieldExpressionMethod((
        KeyValuePair<Guid, SourceModel> source,
        KeyValuePair<Guid, SourcePairModel> sourcePair) scope)
    {
        string methodName = GetCreateUserFieldExpressionMethodName(scope);
        _sb.AppendLine($"private Expression {methodName}(");
        _sb.AppendLine("Expression expression, Expression filter, bool continueWithZero)");
        _sb.AppendLine("{");

        //проверка Expression на тип
        string sourceRecordType = GetRecordClassName(_dataManager.Corrector.Correct(scope.source.Value.Tag));
        var anotherSourceId = _project.GetAnotherSourceId(scope.source.Key, scope.sourcePair.Key);
        var anotherSource = _project.GetSourceOrDefault(anotherSourceId);
        string anotherSourceRecordType = GetRecordClassName(_dataManager.Corrector.Correct(anotherSource.Tag));

        string linkItemsPropertyName = GetLinkedItemsPropertyName(scope.sourcePair.Value);

        _sb.AppendLine(
            $"Expression<Func<{sourceRecordType}, IEnumerable<{anotherSourceRecordType}>>> items = x => x.{linkItemsPropertyName};");
        _sb.AppendLine($"var userFieldExpression = expression as Expression<Func<{sourceRecordType}, object>>;");
        _sb.AppendLine($"var userFieldFilter = filter as Expression<Func<{anotherSourceRecordType}, bool>>;");
        _sb.AppendLine();
        _sb.AppendLine($"var exp1 = items.UserField(userFieldExpression, userFieldFilter, continueWithZero);");

        _sb.AppendLine("return exp1;");
        _sb.AppendLine("}");
    }

    public void AppendReplaceSurrogates(string sourceName, Guid sourceId)
    {
        var mainType = GetRecordClassName(sourceName);

        _sb.AppendLine($"\t\tpublic Expression<Func<{mainType},TResult>> {mainType}ReplaceSurrogates<TResult>(Expression<Func<{mainType},TResult>> expression, DbContext ctx)");
        _sb.AppendLine("\t\t{");

        _sb.AppendLine("if (expression == null) return expression;");

        _sb.AppendLine("expression = expression");

        //делаем ReplaceSurrogate для связей
        foreach (var (linkedItemsPropertyName, _, _) in _dataManager.GetSourceLinkedItems(sourceId)
            .GroupBy(x => x.linkedItemsPropertyName)
            .Select(y => y.First()))//todo solve it one way or ПИХНУТь в него
        {
            // Expression<Func<TMain, TExt>>
            var expressionFrom = $"x => x.{linkedItemsPropertyName}";
            // Expression<Func<TMain, TExt>>
            var expressionTo =
                $"{GetProjectClassName()}.Get{GetStaticClassName(sourceName)}_{linkedItemsPropertyName}Subquery(ctx)";
            _sb.AppendLine($".ReplaceSurrogate({expressionFrom}, {expressionTo})");
        }

        _sb.Append(_codeBuilder.GetSource(sourceId).AppendReplaceSurrogatesMethodContent, 0);
        _sb.AppendLine($";");

        //для всех индикаторов других источников
        var n = 0;
        foreach (var (_, _, otherSourceId) in _dataManager.GetSourceLinkedItems(sourceId)
                .GroupBy(x => x.linkedItemsPropertyName)
                .Select(y => y.First()))
        {
            var (_, otherSourceName, _) = _dataManager.GetSources().FirstOrDefault(x => x.id == otherSourceId);
            foreach (var (_, indicatorName, _) in _dataManager.GetSourceIndicators(otherSourceId))
            {
                if (_codeBuilder.FindSourcePairIndicator(indicatorName) != default) continue;
                ////extract
                _sb.AppendLine(
                    $"    Expression<Func<{GetRecordClassName(otherSourceName)}, Guid?>> one{n} = x => x.{indicatorName};");
                _sb.AppendLine(
                    $"    Expression<Func<{GetRecordClassName(otherSourceName)}, Guid?>> another{n} = {GetProjectClassName()}.Get{GetIndicatorStaticClassName(indicatorName)}IndicatorSubquery(ctx);");
                _sb.AppendLine(
                    $"    expression = Expression.Lambda<Func<{GetRecordClassName(sourceName)}, TResult>>(");
                _sb.AppendLine(
                    $"        new RRExpressionVisitor<{GetRecordClassName(otherSourceName)}, Guid?>(one{n}, another{n}).Visit(expression.Body), expression.Parameters[0]);");

                n++;
            }
        }

        _sb.AppendLine("\t\treturn expression;");
        _sb.AppendLine("\t\t}");
        _sb.AppendLine();
    }

    void AppendQueryBuilder(string sourceName)
    {
        _sb.AppendLine($"\t\tprivate IQueryable<IndicatorResult> {GetQueryBuilderFunctionName(sourceName)}( DbContext ctx, IEnumerable<(Guid ruleId, Expression expression)> rules0)");
        _sb.AppendLine("\t\t{");

        _sb.AppendLine("var rules = rules0.Select(rule =>");

        _sb.AppendLine(
            @$"
                (
                    rule.ruleId,
                    {GetRecordClassName(sourceName)}ReplaceSurrogates((Expression<Func<{GetRecordClassName(sourceName)}, bool>>)rule.expression, ctx)
                )");
        _sb.AppendLine(").ToArray();");

        _sb.AppendLine($@"

            var selector = rules.ToSelector();

            var query = ctx.Set<{GetRecordClassName(sourceName)}>().Select(selector).Where(x => x.RuleId != Guid.Empty).Select(x => new IndicatorResult
            {{ Id = x.Id, RuleId = x.RuleId.ToString() }});

            //заменяем суррогаты на подзапросы
            return query;
");

        _sb.AppendLine("}");
    }

    public void AppendIndicator(string sourceName, string indicatorName, Guid indicatorId)
    {
        _sb.AppendLine(
            $"\tpublic static readonly {Types.GetNetTypeString(typeof(Guid))} {indicatorName}IndicatorId = Guid.Parse(\"{indicatorId}\");");

        _sb.AppendLine(
            $"\tpublic static Expression<Func<{sourceName}Record, Guid?>> Get{indicatorName}IndicatorSubquery(DbContext ctx)");
        //открывающая Expression
        _sb.AppendLine("\t{");

        var indicator = _project.GetIndicatorOrDefault(indicatorId);
        if (indicator.Type == IndicatorTypes.SourcePair)
        {
            _sb.AppendLine($"\treturn null;");
        }
        else
        {
            _sb.AppendLine($"\treturn x =>");
            _sb.AppendLine($"\tctx.Set<{GetSingleCollisionRecordClassName()}>()");
            _sb.AppendLine(
                $"\t.Where(y => y.SourceId == Project.{sourceName}SourceId && y.IndicatorId == {indicatorName}IndicatorId && y.RowId == x.Id)");
            _sb.AppendLine($"\t.Select(y => y.IndicatorRuleId)");
            _sb.AppendLine($"\t.FirstOrDefault();");
        }

        _sb.AppendLine("\t}");
        _sb.AppendLine();
    }

    public void AppendDbContextClass()
    {
        var (sourcePairConnections, sourceCollisions, pairCollisions, _, _) =
            _dataManager.GetVersion(_dataManager.TheVersion);

        _sb.AppendLine($"public class {GetDbContextClassName(_dataManager.TheVersion)} : DbContext");
        //откр класса
        _sb.AppendLine("{");

        _sb.AppendLine(
            $"\tpublic {GetDbContextClassName(_dataManager.TheVersion)}(DbContextOptions options) : base(options) {{ }}");

        _dateConstants = AppendDatesConstants();
        _sb.Append(_dateConstants);

        var constants = _dataManager.GetConstants();
        foreach (var (_, name, type, _) in constants)
        {
            _sb.AppendLine($"\tpublic {Types.GetNetTypeString(type)} {name} {{get {{return Project.{name};}} }}");
        }

        _sb.AppendLine("\tprotected override void OnModelCreating(ModelBuilder modelBuilder)");

        _sb.AppendLine("\t{");
        _sb.AppendLine($"\tmodelBuilder.ApplyConfiguration(new UnknownTableRecordConfiguration());");

        _sb.AppendLine(
            $"\tmodelBuilder.ApplyConfiguration(new PairConnectionRecordConfiguration(\"{sourcePairConnections}\"));");

        _sb.AppendLine(
            $"\tmodelBuilder.ApplyConfiguration(new SingleCollisionRecordConfiguration(\"{sourceCollisions}\"));");
        _sb.AppendLine(
            $"\tmodelBuilder.ApplyConfiguration(new PairCollisionRecordConfiguration(\"{pairCollisions}\"));");

        foreach (var (_, sourceName, sourceId) in _dataManager.GetSources())
        {
            var configClassName = GetRecordConfigurationClassName(sourceName);
            var tableName = _dataManager.GetSchedule(sourceId, _dataManager.TheVersion);

            if (tableName == default)
            {
                tableName = $"table_of_{sourceName}";
            }

            _sb.AppendLine($"\tmodelBuilder.ApplyConfiguration(new {configClassName}(\"{tableName}\"));");
        }

        _sb.AppendLine(
            @$"modelBuilder.HasDbFunction(ModelBuilderExtensions.DateDiffDaysMethodInfo)
            .HasTranslation(args =>
                    new SqlFunctionExpression(""DATE_PART"",
            new SqlExpression[]
            {{
                new SqlConstantExpression(
                    Expression.Constant(""day""),
                    new StringTypeMapping(""string"", DbType.String)
                ),
                new SqlBinaryExpression(
                    ExpressionType.Subtract,
                    args.First(),
                    args.Skip(1).First(),
                    args.First().Type,
                    args.First().TypeMapping)
            }},
            nullable: true,
            argumentsPropagateNullability: new[] {{ true, true }},
            type: typeof(int?),
            typeMapping: new IntTypeMapping(""int"", DbType.Int32))
                );"
            );

        //закр метода
        _sb.AppendLine("\t}");

        //закр класса
        _sb.AppendLine("}");
        _sb.AppendLine();
    }

    StringBuilder AppendDatesConstants()
    {
        StringBuilder sb = new();
        var (_, _, _, calculationDate, actualAtList) =
            _dataManager.GetVersion(_dataManager.TheVersion);
        var dateTimeType = Types.GetNetTypeString(typeof(DateTime));

        sb.AppendLine(
            $"\tpublic {dateTimeType} {CalculatedAt} => new DateTime({calculationDate:yyyy, MM, dd, HH, mm, sss}, DateTimeKind.Utc);");

        foreach (var (_, sourceTag, sourceId) in _dataManager.GetSources())
        {
            sb.AppendLine(
                $"\tpublic {dateTimeType} {sourceTag}{ActualAt} => new DateTime({actualAtList[sourceId]:yyyy, MM, dd, HH, mm, sss}, DateTimeKind.Utc);");
        }

        return sb;
    }

    #region Utils

    public static string GetPairConnectionRecordClassName() => "PairConnectionRecord";

    public static string GetSingleCollisionRecordClassName() => "SingleCollisionRecord";

    public static string GetDbContextClassName(Guid versionId) => $"DbContext_{versionId:N}";

    public static string GetQueryBuilderFunctionName(string sourceName) => $"{sourceName}QueryBuilder";

    public static string GetProjectClassName() => "Project";

    public static string GetRecordClassName(string sourceName) => $"{sourceName}Record";

    public static string GetRecordConfigurationClassName(string sourceName) => $"{GetRecordClassName(sourceName)}Configuration";

    public static string GetStaticClassName(string sourceName) => sourceName;

    public string GetPropertyName(SourceAttributeModel attribute) => _dataManager.Corrector.Correct(attribute.Name);

    public string GetIndicatorStaticClassName(string indicatorName) => _dataManager.Corrector.Correct(indicatorName);

    public string GetLinkedItemsPropertyName(SourcePairModel pair) => _dataManager.Corrector.Correct(pair.Name) + "Items";

    public static string GetProjectConstantName(ProjectParameterModel parameter) => parameter.Tag;

    static readonly IDictionary<Type, Func<string, string>> _map = new Dictionary<Type, Func<string, string>>()
    {
        {typeof(Guid), x => $"{Types.GetNetTypeString(typeof(Guid))}.Parse(\"{x}\")"},
        {typeof(string), x => $"\"{x}\""},
        {typeof(int?), x => x}
    };

    #endregion

    public string GetRecordPropertyName(UserFieldModel userField) => _dataManager.Corrector.Correct(userField.Name);

    public static string GetTypeName<T>()
    {
        var provider = new Microsoft.CSharp.CSharpCodeProvider();
        var ctr = new CodeTypeReference(typeof(T));
        string typeName = provider.GetTypeOutput(ctr);
        return typeName;
    }


}
