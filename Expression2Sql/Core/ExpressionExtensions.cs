using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;

namespace QueryingCore.Core;

public static class ExpressionExtensions
{
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

    sealed class Replacer : ExpressionVisitor
    {
        readonly Expression _one;

        readonly Expression _another;

        public Replacer(Expression one, Expression another) => (_one, _another) = (one, another);

        public override Expression? Visit(Expression? node) => node == _one ? _another : base.Visit(node);
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

    public static Expression<Func<TSource, IndicatorResultInternal>> ToSelector<TSource>(
        this IEnumerable<(Guid ruleId, Expression<Func<TSource, bool>> expression)> rules)
        where TSource : RecordBase
    {
        Expression<Func<TSource, Guid>> caseExpression = r => Guid.Empty;
        ParameterExpression recordParameter = caseExpression.Parameters[0];
        foreach (var (ruleId, expression) in rules.Reverse())
        {
            Expression<Func<bool, Guid, Guid, Guid>> template = (rule, one, another) => rule ? one : another;

            var ruleBody = expression.Body.Replace(expression.Parameters[0], recordParameter);

            var body = template.Body.Replace(template.Parameters[0], ruleBody)
                .Replace(template.Parameters[1], Expression.Constant(ruleId))
                .Replace(template.Parameters[2], caseExpression.Body);

            caseExpression = Expression.Lambda<Func<TSource, Guid>>(body, recordParameter);
        }        

        Expression<Func<TSource, Guid, DateTime, IndicatorResultInternal>> selectorBodyTemplate =
            (p0, p1, p2) => new IndicatorResultInternal {Id = p0.Id, RuleId = p1, Date = p2};

        var selectorBody = selectorBodyTemplate.Body
            .Replace(selectorBodyTemplate.Parameters[0], recordParameter)
            .Replace(selectorBodyTemplate.Parameters[1], caseExpression.Body);

        return Expression.Lambda<Func<TSource, IndicatorResultInternal>>(selectorBody, recordParameter);
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

    public static Expression<Func<TMain, TResult>> NormalizeConstants<TMain, TResult>(
        this Expression<Func<TMain, TResult>> source)
    {
        if (source == null)
        {
            throw new ArgumentNullException(nameof(source));
        }

        return Expression.Lambda<Func<TMain, TResult>>(new ConstantsResolver().Visit(source.Body),
            source.Parameters[0]);
    }
    public static Expression<Func<TP1, TP2, TResult>> NormalizeConstants<TP1, TP2, TResult>(
       this Expression<Func<TP1, TP2, TResult>> source)
    {
        if (source == null)
        {
            throw new ArgumentNullException(nameof(source));
        }

        return Expression.Lambda<Func<TP1,TP2, TResult>>(new ConstantsResolver().Visit(source.Body),
            source.Parameters[0], source.Parameters[1]);
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


    public interface IS
    {
        LambdaExpression Create(Expression body, ParameterExpression parameter);
    }

    public class S<TMain, TResult> : IS
        where TMain : RecordBase
    {
        public LambdaExpression Create(Expression body, ParameterExpression parameter)
        {
            return Expression.Lambda<Func<TMain, TResult>>(body, parameter);
        }
    }

    public static Expression CreateUserFieldExpression(
        this IQueryingContextAggregative context,
        Type type,
        Expression expression,
        Expression filter,
        bool continueWithZero)
    {
        return (Expression)
            typeof(IQueryingContextAggregative)
                .GetMethod(nameof(IQueryingContextAggregative.CreateUserFieldExpression))
                .MakeGenericMethod(type)
                .Invoke(context, new object[] {expression, filter, continueWithZero});
    }

    public static IQueryable CreateUserFieldQuery(
        this IQueryingContextAggregative context,
        Type type,
        DbContext dbContext,
        Expression expression)
    {
        return (IQueryable)
            typeof(IQueryingContextAggregative)
                .GetMethod(nameof(IQueryingContextAggregative.CreateUserFieldQuery))
                .MakeGenericMethod(type)
                .Invoke(context, new object[] {dbContext, expression});
    }

    public static IList ToUserFieldList(
        this IQueryable queryable,
        Type type)
    {
        return (IList)
            typeof(ExpressionExtensions)
                .GetMethod(nameof(ToUserFieldList1))
                .MakeGenericMethod(type)
                .Invoke(null, new object[] {queryable});
    }

    public static IList<UserFieldResult<TResult>> ToUserFieldList1<TResult>(
        this IQueryable queryable)
    {
        return (queryable as IQueryable<UserFieldResult<TResult>>).ToList();
    }
}

public class UserFieldResult<TResult>
{
    public int Id { get; set; }
    public TResult Value { get; set; }
}
