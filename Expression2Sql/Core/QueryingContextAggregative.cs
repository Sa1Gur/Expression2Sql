using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace QueryingCore.Core;

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
