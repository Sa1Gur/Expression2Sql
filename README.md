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

10. Как будут выглядеть ошибки? Вот так. Как можно отладить? Например так. Можно вставить заменённый код прямо в проект. Что неудобно. А что ещё? Ещё можно использовать сорсгенерацию

11. Загружение сборку в память

12. Теперь попробуем получить SQL. Для этого нужно пересобрать выражение

13. Используем Visitor для замены некоторых частей выражений

14. Компилируем новое выражение и сформировать SQL при помощи QueryString

15. Немного про добавление кастомных выражений в EF (скорее всего будет в другом докладе)

16. Выгружаем сбору из памяти

17. Теперь у нас есть SQL и думаем как его тестировать. Например, можно использовать сравнением строк. Чем плохо? Очень зависит от изменений в EF

18. Можно тестировать сразу на данных. Хорошо т.к. не завсит от изменений EF, но получается не чистый модульный тест. "Про то, как эффективно протестировать, можно узнать в другом докладе".
