#### Инструменты преобразования бизнес выражений к SQL запросам.

По модели данных генерируется C# код сборки для работы с базой данных через Entity Framework.
C# код компилируется в сборку, сборка подгружается в процесс.
Выражения в бизнес синтаксисе преобразуются к Lambda выражениям в EF контексте сгенерированной сборки.
Lambda выражения компилируются и средствами Entity Framework преобразуются к SQL запросам.

Все преобразования осуществляются по модели данных (загрузка данных не требуется).

##### Возможности:
- преобразование строк выражений в простом синтаксисе (SympleSintax) к строкам Lambda выражений.
- компиляция правил индикаторов, фильтров и выражений пользовательских полей в Lambda выражения в EF контексте проекта базы данных.
- преобразование Lambda выражений в SQL запросы

##### Архитектура решения
- Для динамической компиляции кода используется [Microsoft .NET Compiler Platform](https://docs.microsoft.com/dotnet/csharp/roslyn-sdk/).
- Для доступа к данным используется [EF Core](https://docs.microsoft.com/ef/core/) с провайдером доступа [PostgreSQL](http://www.npgsql.org/efcore/index.html).
- Для анализа и преобразования [Lambda выражений](https://docs.microsoft.com/dotnet/csharp/language-reference/operators/lambda-expressions) применяется паттерн [Visitor](https://docs.microsoft.com/dotnet/api/system.linq.expressions.expressionvisitor).

0 Обо мне 
я это просто я. Иногда говорят, что докладчик не нуждается в представлении, т.к. его все хорошо знают. В данном случае все наоборот - представление ещё рано

1 Задача
есть таблица. Мы хотим предоставить пользователю возможность составлять выражения в своём синтаксисе так, чтобы они вычисллялись в базе данных

2 Подходы
можно ли попробовать составлять SQL самому. Сделать разбор синтаксиса и генерировать SQL. Звучит довольно сложно (сделать разбор выражения сложно само по себе плюс).

3. Для разбора выражений можно использовать Roslyn. Т.е. можем ввести сурогатный синтаксис, а потом его преобразовать в Lambda. Уже при помощи Roslyn создадим Expression из Lambda

4. Теперь можем приступить к формированию SQL. Если мы можем формировать SQL по Expression - мы фактически делаем ORM. Но ORM у нас уже есть.

5. Как использовать EF для генерации? Собственно точно также, как и использовать его. Создаем Record с описанием схемы данных и добавляем туда наше выражение. А потом генерируем SQL при помощи QueryString

6. А что делать раньше? Раньше можно было получить SQL из exception

7. Пример, выражения такое и такое

8. Как тогда будет выглядеть наш контекст:
вот он

9. Задаём выражение в виде и компилируем.

public static Expression CSharpScriptEvaluate(Type typeWrapper, string expressionName = "expr")
{
    var exprField = typeWrapper.GetField(expressionName, BindingFlags.Public | BindingFlags.Instance);
    var main = Activator.CreateInstance(typeWrapper);

    var expressionBoxed = exprField.GetValue(main) as LambdaExpression;

    var expressionUnboxed = Unbox(expressionBoxed.Body);

    var sInstance = (IS)Activator.CreateInstance(typeof(S<,>).MakeGenericType(typeof(TMain), expressionUnboxed.Type));
    return sInstance.Create(expressionUnboxed, expressionBoxed.Parameters[0]);
    //return Expression.Lambda<Func<TMain>> (expressionUnboxed, expressionBoxed.Parameters[0]);
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

10. Как будут выглядеть ошибки? Вот так. Как можно отладить? Например так. Можно вставить заменённый код прямо в проект. Что неудобно. А что ещё? Ещё можно использовать сорсгенерацию

11. Загружение сборку в память

Type type = assembly.GetTypes().FirstOrDefault(x => x.IsSubclassOf(typeof(FactoryBase)));
expressionScope._factoryBase = (FactoryBase) Activator.CreateInstance(type);

const string? fakeConnectionString = "Host=_;Database=_;Username=_;Password=_);";
var options = new DbContextOptionsBuilder()
    .UseNpgsql(fakeConnectionString)
    .Options;

var dbContextClassName = CodeGenerator.GetDbContextClassName(_dataManager.TheVersion);
var type = Factory.GetType();
Type dbContextType = type.Assembly.GetType($"{type.Namespace}.{dbContextClassName}");
Ctx = (DbContext) Activator.CreateInstance(dbContextType, (object) options);

12. Теперь попробуем получить SQL. Для этого нужно пересобрать выражение

13. Используем Visitor для замены некоторых частей выражений

14. Компилируем новое выражение и сформировать SQL при помощи QueryString

15. Немного про добавление кастомных выражений в EF (скорее всего будет в другом докладе)

16. Выгружаем сбору из памяти

17. Теперь у нас есть SQL и думаем как его тестировать. Например, можно использовать сравнением строк. Чем плохо? Очень зависит от изменений в EF

18. Можно тестировать сразу на данных. Хорошо т.к. не завсит от изменений EF, но получается не чистый модульный тест. "Про то, как эффективно протестировать, можно узнать в другом докладе".

19. Утечки

20.
https://github.com/dotnet/roslyn/issues/41722

Every CSharpScript.EvaluateAsync() call generates a new assembly that does not get unloaded #41722
 Open
zeroskyx opened this issue on Feb 15, 2020 · 13 comments
 Open
Every CSharpScript.EvaluateAsync() call generates a new assembly that does not get unloaded
#41722
zeroskyx opened this issue on Feb 15, 2020 · 13 comments
Comments
@zeroskyx
zeroskyx commented on Feb 15, 2020
Using the following code:

while(true)
{
	await Microsoft.CodeAnalysis.CSharp.Scripting.CSharpScript.EvaluateAsync("4 + 8");
	System.Console.WriteLine($"Loaded assemblies: {System AppDomain.CurrentDomain.GetAssemblies().Count()}");
}
Output:

Loaded assemblies: 49
Loaded assemblies: 50
Loaded assemblies: 51
Loaded assemblies: 52
Loaded assemblies: 53
Loaded assemblies: 54
...
The scripting API generates a new assembly for every call that does not get unloaded, resulting in an ever-increasing number of loaded assemblies and thus in an increasing amount of memory consumption.

There does not appear to be a way to unload the generated assemblies since they are not collectible (How to use and debug assembly unloadability in .NET Core).

How can we unload these generated assemblies?

21. https://github.com/dotnet/roslyn/issues/22219
Version Used: 2.3.0.0

I have an application that creates and pre-compiles a number of scripts.
For me though, it seems that ~39 scripts is the limit where compilation will fail with an OOM exception.

var script = CSharpScript.Create(text, scriptOptions);
script.Compile();
My scriptOptions contains 1 assembly reference and 2 imports.
Each script compilation consumes roughly over 50mb.

Releasing the references to these scripts allows the gc to free up memory and not get an OOM ex meaning each script reference seems to consume roughly the same amount of memory as the entire main application without scripts. This seems slightly excessive.

I will help with any details needed. All help is welcome.

