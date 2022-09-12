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
