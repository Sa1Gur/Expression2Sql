using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Metadata;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using static ExpressionTranslator.ExpressionCoverter;

namespace ExpressionTranslator;

public class ExpressionCoverter
{
    public static Guid RDLSourceId { get; private set; }
    public static Guid MTOLSourceId { get; private set; }

    public Expression Convert(string expression)
    {
        string a = ConvertToLambdaSyntax(expression);
        GetExpression();
        return null;
    }

    private string ConvertedToLambda(string lambda)
    {
        if (lambda.Contains("=>")) return lambda;

        return  ConvertToLambdaSyntax(lambda);
    }

    private string ConvertToLambdaSyntax(string rule)
    {
        /*rule = col_eq.Replace(rule, RuleEvaluator);
        
        }*/

        return $"x => {rule};";
    }

    LambdaExpression GetExpression()
    {
        QueryingContextAggregative<RDLRecord, MTOLRecord> expressionContext = new (x => x.RDMTOItems,
                GetRDL_RDMTOItemsSubquery);
        var expressionLambda = expressionContext.Evaluate(typeof(RecordBase.ExpressionWrapperType));

        var expressionInvocationResult = typeof(IQueryingContextAggregative)
            .GetMethod(nameof(IQueryingContextAggregative.CreateUserFieldExpression))?
            .MakeGenericMethod(expressionLambda.Body.Type)?
            .Invoke(expressionContext, new object[] { expressionLambda, null, true });

        //return (LambdaExpression)expressionInvocationResult;
        return null;
    }

    public string? Sql<TMain, TResult>(Expression<Func<TMain, TResult>> expression)
        where TMain : RecordBase
    {
        /*var ctx = _expressionScope.Ctx;
        var query = ctx.Set<TMain>().Select(_factory.ReplaceSurrogates(expression, ctx).SelectResult().NormalizeConstants());

        var sql = query
            .Where(_ => ctx.Set<UnknownTableRecord>().Any(y => y.Id == y.Id))
            .ToQueryString();

        return sql?.Substring(0, sql.LastIndexOf("WHERE ", StringComparison.Ordinal));*/
        return "";
    }

    public class RDLRecord : RecordBase
    {
        public string Code1 { get; set; }
        public DateTime? Fact1 { get; set; }
        public string Name1 { get; set; }
        public int? Number1 { get; set; }
        public DateTime? Plan1 { get; set; }

        [RuleDependency("00000000-2001-0000-0000-000000000000")]
        public virtual ICollection<MTOLRecord> RDMTOItems { get; set; }

        [RuleDependency("00000000-3001-0000-0000-000000000000")]
        public Guid? RDunique { get; set; }

        [RuleDependency("00000000-3002-0000-0000-000000000000")]
        public Guid? RDunique1 { get; set; }

        [RuleDependency("00000000-3003-0000-0000-000000000000")]
        public Guid? yellow { get; set; }

        // User fields (1)
        [RuleDependency("00000000-4001-0000-0000-000000000000")]
        public string RDuserfield1 { get; set; }

        [RuleDependency("00000000-3007-0000-0000-000000000000")]
        public ICollection<Guid> PairIndicator1Items { get; set; }
    }

    public class MTOLRecord : RecordBase
    {
        public string Code { get; set; }
        public DateTime? Fact { get; set; }
        public string NameEq { get; set; }
        public int? Number { get; set; }
        public DateTime? Plan { get; set; }

        [RuleDependency("00000000-2001-0000-0000-000000000000")]
        public virtual ICollection<RDLRecord> RDMTOItems { get; set; }

        [RuleDependency("00000000-3004-0000-0000-000000000000")]
        public Guid? MTOAggregative { get; set; }

        [RuleDependency("00000000-3006-0000-0000-000000000000")]
        public Guid? MTOunique { get; set; }

        // User fields (1)
        [RuleDependency("00000000-4002-0000-0000-000000000000")]
        public string MTOuserfield1 { get; set; }

        [RuleDependency("00000000-3007-0000-0000-000000000000")]
        public ICollection<Guid> PairIndicator1Items { get; set; }
    }

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class RuleDependencyAttribute : Attribute
    {
        public RuleDependencyAttribute(string objectId) => ObjectId = Guid.Parse(objectId);

        public Guid ObjectId { get; }
    }

    public static Expression<Func<RDLRecord, IEnumerable<MTOLRecord>>> GetRDL_RDMTOItemsSubquery(DbContext ctx)
    {
        Guid RDMTOItemsId = Guid.Parse("00000000-2001-0000-0000-000000000000");
        var oneInPair = ctx.Set<PairConnectionRecord>()
            .Where(x => x.SourcePairId == RDMTOItemsId && x.SourceId == RDLSourceId);
        var anotherInPair = ctx.Set<PairConnectionRecord>()
            .Where(x => x.SourcePairId == RDMTOItemsId && x.SourceId == MTOLSourceId);
        var p = oneInPair
            .Join(anotherInPair, sp2 => sp2.Link, sp3 => sp3.Link, (sp2, sp3) => new { sp2.RowId, extId = sp3.RowId })
            .Join(ctx.Set<MTOLRecord>(), p => p.extId, p => p.Id, (s, ext) => new { s.RowId, ext });
        return x => p.Where(z => z.RowId == x.Id).Select(x => x.ext);
    }

    public class PairConnectionRecord
    {
        public Guid SourcePairId { get; set; }
        public Guid SourceId { get; set; }
        public int RowId { get; set; }
        public int Link { get; set; }
        public PairConnectionType ConnectionType { get; set; }
    }

    public enum PairConnectionType
    {
        Connected = 0,
        Unconnected = 1
    }

    public interface IQueryingContextAggregative : IQueryingContext
    {
        Expression CreateUserFieldExpression<TResult>(
            Expression expression,
            Expression filter,
            bool continueWithZero);

        IQueryable<UserFieldResult<TResult>> CreateUserFieldQuery<TResult>(
            DbContext dbContext,
            Expression expression,
            ILogger logger);
    }
    public class QueryingContextAggregative<TMain, TExt> : QueryingContext<TMain>, IQueryingContextAggregative
    where TMain : RecordBase
    where TExt : RecordBase
    {
        public QueryingContextAggregative(
            Expression<Func<TMain, IEnumerable<TExt>>> items,
            Func<DbContext, Expression<Func<TMain, IEnumerable<TExt>>>> itemsSubquery)
        {
            _items = items;
            _itemsSubquery = itemsSubquery;
        }
        public Expression CreateUserFieldExpression<TResult>(
            Expression expression,
            Expression filter,
            bool continueWithZero)
        {
            if (expression == null)
            {
                throw new ArgumentNullException(nameof(expression));
            }
            var userFieldExpression = expression as Expression<Func<TMain, TResult>>;
            if (userFieldExpression == null)
            {
                throw new ArgumentException("Unexpected type", nameof(expression));
            }
            var userFieldFilter = filter as Expression<Func<TExt, bool>>;
            if (filter != null && userFieldFilter == null)
            {
                throw new ArgumentException("Unexpected type", nameof(filter));
            }

            return _items.UserField(userFieldExpression, userFieldFilter, continueWithZero);
        }

        public IQueryable<UserFieldResult<TResult>> CreateUserFieldQuery<TResult>(
            DbContext dbContext,
            Expression expression,
            ILogger logger)
        {
            if (dbContext == null)
            {
                throw new ArgumentNullException(nameof(dbContext));
            }
            if (expression == null)
            {
                throw new ArgumentNullException(nameof(expression));
            }
            var userFieldExpression = expression as Expression<Func<TMain, TResult>>;
            if (userFieldExpression == null)
            {
                throw new ArgumentException("Unexpected type", nameof(expression));
            }

            return
                dbContext.Set<TMain>().Select(
                userFieldExpression
                    .ReplaceSurrogate(_items, _itemsSubquery(dbContext))
                    .SelectResult());
        }

        Expression<Func<TMain, IEnumerable<TExt>>> _items;
        Func<DbContext, Expression<Func<TMain, IEnumerable<TExt>>>> _itemsSubquery;
    }

    public interface IQueryingContext
    {
        LambdaExpression Evaluate(Type typeWrapper);
    }

    public class QueryingContext<TMain> : IQueryingContext
        where TMain : RecordBase
    {
        public LambdaExpression Evaluate(Type typeWrapper) => (LambdaExpression)CSharpScriptEvaluate(typeWrapper);

        private static Expression Unbox(Expression expression)
        {
            while (true)
            {
                if (expression.NodeType == ExpressionType.Convert)
                {
                    expression = ((UnaryExpression)expression).Operand;
                    continue;
                }

                return expression;
            }
        }

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
    }

    public class RecordBase
    {
        public int Id { get; set; }

        public class ExpressionWrapperType
        {
            public Expression<Func<RecordBase, object>> expr = x => true;
        }
    }

    public interface IS
    {
        LambdaExpression Create(Expression body, ParameterExpression parameter);
    }

    public class S<TMain, TResult> : IS
        where TMain : RecordBase
    {
        public LambdaExpression Create(Expression body, ParameterExpression parameter) =>
            Expression.Lambda<Func<TMain, TResult>>(body, parameter);
    }
}

public class QueryingContextAggregative<TMain, TExt>
    where TMain : RecordBase
    where TExt : RecordBase
{
    public QueryingContextAggregative(
        Expression<Func<TMain, IEnumerable<TExt>>> items,
        Func<DbContext, Expression<Func<TMain, IEnumerable<TExt>>>> itemsSubquery)
    {
        _items = items;
        _itemsSubquery = itemsSubquery;
    }
    public Expression CreateUserFieldExpression<TResult>(
        Expression expression,
        Expression filter,
        bool continueWithZero)
    {
        if (expression == null)
        {
            throw new ArgumentNullException(nameof(expression));
        }
        if (expression is not Expression<Func<TMain, TResult>> userFieldExpression)
        {
            throw new ArgumentException("Unexpected type", nameof(expression));
        }
        var userFieldFilter = filter as Expression<Func<TExt, bool>>;
        if (filter != null && userFieldFilter == null)
        {
            throw new ArgumentException("Unexpected type", nameof(filter));
        }

        return _items.UserField(userFieldExpression, userFieldFilter, continueWithZero);
    }

    public IQueryable<UserFieldResult<TResult>> CreateUserFieldQuery<TResult>(
        DbContext dbContext,
        Expression expression,
        ILogger logger)
    {
        if (dbContext == null)
        {
            throw new ArgumentNullException(nameof(dbContext));
        }
        if (expression == null)
        {
            throw new ArgumentNullException(nameof(expression));
        }
        var userFieldExpression = expression as Expression<Func<TMain, TResult>>;
        if (userFieldExpression == null)
        {
            throw new ArgumentException("Unexpected type", nameof(expression));
        }

        return
            dbContext.Set<TMain>().Select(
            userFieldExpression
                .ReplaceSurrogate(_items, _itemsSubquery(dbContext))
                .SelectResult());
    }    

    Expression<Func<TMain, IEnumerable<TExt>>> _items;
    Func<DbContext, Expression<Func<TMain, IEnumerable<TExt>>>> _itemsSubquery;
}
public class UserFieldResult<TResult>
{
    public int Id { get; set; }
    public TResult Value { get; set; }
}

public static class ExpressionExtensions
{
    public static Expression<Func<TMain, TResult>> UserField<TMain, TExt, TResult>(
        this Expression<Func<TMain, IEnumerable<TExt>>> items,
        Expression<Func<TMain, TResult>> expression,
        Expression<Func<TExt, bool>>? filter,
        bool continueWithZero)
    {
        if (items == null)
        {
            throw new ArgumentNullException(nameof(items));
        }

        if (expression == null)
        {
            throw new ArgumentNullException(nameof(expression));
        }

        if (filter == null)
        {
            return expression;
        }

        if (!continueWithZero)
        {
            return expression.ReplaceSurrogate(items, items.AddFilter(filter));
        }

        Expression<Func<bool, TResult, TResult, TResult>> template = (condition, one, another) =>
            condition ? one : another;

        var body = template.Body.Replace(template.Parameters[0], items.AddFilter(filter).AddAny().Body)
            .Replace(template.Parameters[1], expression.ReplaceSurrogate(items, items.AddFilter(filter)).Body)
            .Replace(template.Parameters[2], expression.Body);

        return Expression.Lambda<Func<TMain, TResult>>(body, expression.Parameters[0]);
    }

    public static Expression<Func<TMain, IEnumerable<TExt>>> AddFilter<TMain, TExt>(
        this Expression<Func<TMain, IEnumerable<TExt>>> source,
        Expression<Func<TExt, bool>> filter)
    {
        if (source == null)
        {
            throw new ArgumentNullException(nameof(source));
        }

        if (filter == null)
        {
            throw new ArgumentNullException(nameof(filter));
        }

        Expression<Func<IEnumerable<TExt>, Func<TExt, bool>, IEnumerable<TExt>>> template = (x, p) => x.Where(p);

        var body = template.Body.Replace(template.Parameters[0], source.Body);
        body = body.Replace(template.Parameters[1], filter);

        return Expression.Lambda<Func<TMain, IEnumerable<TExt>>>(body, source.Parameters[0]);
    }

    public static Expression<Func<TMain, bool>> AddAny<TMain, TExt>(
        this Expression<Func<TMain, IEnumerable<TExt>>> source)
    {
        if (source == null)
        {
            throw new ArgumentNullException(nameof(source));
        }

        Expression<Func<IEnumerable<TExt>, bool>> template = x => x.Any();

        var body = template.Body.Replace(template.Parameters[0], source.Body);

        return Expression.Lambda<Func<TMain, bool>>(body, source.Parameters[0]);
    }

    sealed class Replacer : ExpressionVisitor
    {
        readonly Expression _one;

        readonly Expression _another;

        public Replacer(Expression one, Expression another) => (_one, _another) = (one, another);

        public override Expression? Visit(Expression? node) => node == _one ? _another : base.Visit(node);
    }

    public static Expression<Func<TMain, UserFieldResult<TResult>>> SelectResult<TMain, TResult>(
        this Expression<Func<TMain, TResult>> source) where TMain : RecordBase
    {
        if (source == null)
        {
            throw new ArgumentNullException(nameof(source));
        }

        Expression<Func<TMain, TResult, UserFieldResult<TResult>>> template = (x, p) =>
            new UserFieldResult<TResult>
            {
                Id = x.Id,
                Value = p
            };

        var body = template.Body
            .Replace(template.Parameters[0], source.Parameters[0])
            .Replace(template.Parameters[1], source.Body);

        var result = Expression.Lambda<Func<TMain, UserFieldResult<TResult>>>(body, source.Parameters[0]);
        return result;
    }

    public static Expression<Func<TMain, T>> ReplaceSurrogate<TMain, TExt, T>(this Expression<Func<TMain, T>> rule,
        Expression<Func<TMain, IEnumerable<TExt>>> one,
        Expression<Func<TMain, IEnumerable<TExt>>> another)
    {
        one = Expression.Lambda<Func<TMain, IEnumerable<TExt>>>(
            one.Body.Replace(one.Parameters[0], rule.Parameters[0]), rule.Parameters[0]);

        another = Expression.Lambda<Func<TMain, IEnumerable<TExt>>>(
            another.Body.Replace(another.Parameters[0], rule.Parameters[0]), rule.Parameters[0]);

        return Expression.Lambda<Func<TMain, T>>(
            new SurrogateReplacer<TMain, TExt>(one, another).Visit(rule.Body), rule.Parameters[0]);
    }

    public static Expression? Replace(this Expression expression, Expression one, Expression another)
    {
        return new Replacer(one, another).Visit(expression);
    }

    public static Expression ReplaceCustomFunction(this Expression expression)
    {
        var lambda = expression as LambdaExpression;
        return
            lambda != null
                ? Expression.Lambda(new CustomFunctionRewriter().Visit(lambda.Body), lambda.Parameters[0])
                : expression;
    }

    public class SurrogateReplacer<TMain, TExt> : ExpressionVisitor
    {
        public SurrogateReplacer(
            Expression<Func<TMain, IEnumerable<TExt>>> surrogate,
            Expression<Func<TMain, IEnumerable<TExt>>> subquery)
        {
            _surrogate = surrogate;
            _subquery = subquery;
        }

        public override Expression? Visit(Expression? node)
        {
            if (node != null && node.ToString() == _surrogate.Body.ToString())
            {
                return _subquery.Body;
            }

            return base.Visit(node);
        }

        readonly Expression<Func<TMain, IEnumerable<TExt>>> _surrogate;
        readonly Expression<Func<TMain, IEnumerable<TExt>>> _subquery;
    }
}

public static class CustomFunctionExtensions
{
    public static bool any_null(params object?[] values) => throw new NotSupportedException("");

    public static bool eq(object? value1, object? value2) => throw new NotSupportedException("");

    public static bool not_eq(object value1, object value2) => throw new NotSupportedException("");

    public static DateTime? date_or_null(DateTime? value) => throw new NotSupportedException("");

    public static DateTime today() => throw new NotSupportedException("");
}

public class CustomFunctionRewriter : ExpressionVisitor
{
    static readonly MethodInfo? AnyNull =
        typeof(CustomFunctionExtensions).GetMethod(nameof(CustomFunctionExtensions.any_null), BindingFlags.Static | BindingFlags.Public);

    static readonly MethodInfo? Eq =
        typeof(CustomFunctionExtensions).GetMethod(nameof(CustomFunctionExtensions.eq), BindingFlags.Static | BindingFlags.Public);

    static readonly MethodInfo? NotEq =
        typeof(CustomFunctionExtensions).GetMethod(nameof(CustomFunctionExtensions.not_eq), BindingFlags.Static | BindingFlags.Public);

    static readonly MethodInfo? DateOrNull =
        typeof(CustomFunctionExtensions).GetMethod(nameof(CustomFunctionExtensions.date_or_null), BindingFlags.Static | BindingFlags.Public);

    static readonly MethodInfo? Today =
        typeof(CustomFunctionExtensions).GetMethod(nameof(CustomFunctionExtensions.today), BindingFlags.Static | BindingFlags.Public);

    protected override Expression VisitMethodCall(MethodCallExpression node)
    {
        if (node.Method == AnyNull)
        {
            if (!(node.Arguments[0] is NewArrayExpression array)) throw new NotImplementedException();

            Expression? result = null;
            foreach (var expression in array.Expressions)
            {
                Expression<Func<object?, bool>> donor = x => x == null;
                var right = (BinaryExpression)donor.Body.Replace(donor.Parameters[0], Visit(expression)!);

                result = result is { } left
                    ? Expression.OrElse(left, right)
                    : right;
            }

            return result!;
        }

        if (node.Method == Eq)
        {
            Expression<Func<object?, object?, bool>> e = (x1, x2) => x1 != null && x2 != null && x1 == x2;

            return e.Body
                .Replace(e.Parameters[0], Visit(node.Arguments[0])!)
                .Replace(e.Parameters[1], Visit(node.Arguments[1])!);
        }

        if (node.Method == NotEq)
        {
            Expression<Func<object?, object?, bool>> e = (x1, x2) => !(x1 != null && x2 != null && x1 == x2);
            return e.Body
                .Replace(e.Parameters[0], Visit(node.Arguments[0])!)
                .Replace(e.Parameters[1], Visit(node.Arguments[1])!);
        }

        if (node.Method == DateOrNull)
        {
            Expression<Func<DateTime?, DateTime?>> e = x => x != null ? x.Value.Date : (DateTime?)null;

            return e.Body.Replace(e.Parameters[0], Visit(node.Arguments[0])!);
        }

        if (node.Method == Today)
        {
            Expression<Func<DateTime>> e = () => DateTime.Today.ToUniversalTime();

            return e.Body;
        }

        return base.VisitMethodCall(node);
    }
}
