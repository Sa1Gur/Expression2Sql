using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using QueryingCore.AggregativeIndicatorCore;
using Microsoft.EntityFrameworkCore;

namespace QueryingCore.Core;

public delegate Expression RuleValidatorDelegate(Type typeWrapper, string ruleId);

public delegate IQueryable<IndicatorResult> IndicatorQueryBuilderDelegate(DbContext ctx,
    IEnumerable<(Guid ruleId, Expression ruleExpression)> rules);

public abstract class FactoryBase
{
    protected IDictionary<Guid, Type> RuleTypes { get; } = new ConcurrentDictionary<Guid, Type>();

    protected IDictionary<Guid, RuleValidatorDelegate> RuleEvaluators { get; } = new ConcurrentDictionary<Guid, RuleValidatorDelegate>();

    protected IDictionary<Guid, IndicatorQueryBuilderDelegate> IndicatorQueryBuilders { get; } = new ConcurrentDictionary<Guid, IndicatorQueryBuilderDelegate>();

    protected IDictionary<Guid, IQueryingContext> QueryingContexts { get; } = new ConcurrentDictionary<Guid, IQueryingContext>();

    protected IDictionary<(Guid, Guid), IQueryingContextAggregative> QueryingContextAggregatives { get; } = new ConcurrentDictionary<(Guid, Guid), IQueryingContextAggregative>();

    protected void AddContext(Guid key, IQueryingContext context)
    {
        if (!QueryingContexts.ContainsKey(key))
            QueryingContexts.TryAdd(key, context);
        else
            QueryingContexts[key] = context;
    }

    protected void AddContext((Guid, Guid) key, IQueryingContextAggregative context)
    {
        if (!QueryingContextAggregatives.ContainsKey(key))
            QueryingContextAggregatives.TryAdd(key, context);
        else
            QueryingContextAggregatives[key] = context;
    }

    public virtual Expression<Func<TSource, TResult>> ReplaceSurrogates<TSource, TResult>(Expression<Func<TSource, TResult>> lambda, DbContext ctx) =>
        (Expression<Func<TSource, TResult>>)GetType()
        .GetMethod($"{typeof(TSource).Name}ReplaceSurrogates")
        .MakeGenericMethod(typeof(TResult))
        .Invoke(this, new object[] { lambda, ctx });

    public Expression EvaluateRule(Guid sourceId, Type typeWrapper, Guid ruleId)
    {
        if (!RuleEvaluators.ContainsKey(sourceId))
            throw new ArgumentException("Неизвестный идентификатор источника", nameof(sourceId));            

        return RuleEvaluators[sourceId](typeWrapper, $"_{ruleId:N}");
    }    

    public IQueryable<IndicatorResult> GetQuery(DbContext ctx, Guid sourceId, IEnumerable<(Guid ruleId, Expression ruleExpression)> rules)
    {
        if (!IndicatorQueryBuilders.ContainsKey(sourceId))
            throw new ArgumentException("Неизвестный идентификатор источника", nameof(sourceId));

        var visitedRules = rules.Select(r => (r.ruleId, r.ruleExpression.ReplaceCustomFunction()));
        
        return IndicatorQueryBuilders[sourceId](ctx, visitedRules);
    }

    public IQueryingContextAggregative GetContext((Guid sourceId, Guid sourcePairId) context) =>
        QueryingContextAggregatives.ContainsKey(context) ? QueryingContextAggregatives[context] : throw new ArgumentOutOfRangeException(nameof(context));

    public IQueryingContext GetContext(Guid sourceId) =>
        QueryingContexts.ContainsKey(sourceId) ? QueryingContexts[sourceId] : throw new ArgumentOutOfRangeException(nameof(sourceId));
}
