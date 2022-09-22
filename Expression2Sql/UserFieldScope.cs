using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using QueryingCore.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.CodeAnalysis.Scripting;
using RestApi.Data.Models;

namespace QueryingCore;

public sealed class UserFieldScope
{
    readonly ExpressionScope _expressionScope;
    readonly FactoryBase _factory;
    readonly Guid _sourceId;
    readonly Guid _anotherSourceId;
    readonly Guid _sourcePairId;
    readonly ProjectModel _project;
    readonly Guid _versionId;

    public UserFieldScope(ExpressionScope expressionScope, FactoryBase factory, Guid sourceId, Guid anotherSourceId, Guid sourcePairId, ProjectModel project, Guid versionId)
    {
        _expressionScope = expressionScope;
        _factory = factory;
        _sourceId = sourceId;
        _anotherSourceId = anotherSourceId;
        _sourcePairId = sourcePairId;
        _project = project;
        _versionId = versionId;
    }    

    LambdaExpression GetExpression(bool continueWithZero)
    {
        var expressionContext = _expressionScope.Factory.GetContext((_sourceId, _sourcePairId));
        
        var expressionLambda = expressionContext.Evaluate(_expressionScope.ExpressionWrapperType);
        
        if (Types.GetDataType(expressionLambda.Body.Type) == DataTypes.Unknown)
            throw new InvalidOperationException($"UnexpectedExpressionType: {expressionLambda.Body.Type}");            

        LambdaExpression? filterLambda = null;

        if (_expressionScope.FilterWrapperType is not null)
        {
            var filterContext = _expressionScope.Factory.GetContext(_anotherSourceId);

            filterLambda = filterContext.Evaluate(_expressionScope.FilterWrapperType);
            
            if (filterLambda.Body.Type != typeof(bool))
                throw new InvalidOperationException($"UnexpectedFilterExpressionType: {filterLambda.Body.Type}");                
        }

        var expressionInvocationResult = typeof(IQueryingContextAggregative)
            .GetMethod(nameof(IQueryingContextAggregative.CreateUserFieldExpression))?
            .MakeGenericMethod(expressionLambda.Body.Type)?
            .Invoke(expressionContext, new object[] { expressionLambda, filterLambda, continueWithZero });

        if (expressionInvocationResult == null) throw new InvalidOperationException("SimpleSyntax");
        
        return (LambdaExpression)expressionInvocationResult;
    }

    public string? Sql<TMain, TResult>(Expression<Func<TMain, TResult>> expression)
        where TMain : RecordBase
    {
        var ctx = _expressionScope.Ctx;
        var query = ctx.Set<TMain>().Select(_factory.ReplaceSurrogates(expression, ctx)
            .SelectResult());
            //.NormalizeConstants());

        var sql = query
            .Where(_ => ctx.Set<UnknownTableRecord>().Any(y => y.Id == y.Id))
            .ToQueryString();

        return  sql?[..sql.LastIndexOf("WHERE ", StringComparison.Ordinal)];
    }

    public string GetUpdateSql(bool continueWithZero, string columnName)
    {
        var lambda = GetExpression(continueWithZero);

        lambda = Expression.Lambda(new CustomFunctionRewriter().Visit(lambda.Body), lambda.Parameters[0]);
        
        var sql = (string)typeof(UserFieldScope)
            .GetMethod(nameof(Sql))
            .MakeGenericMethod(lambda.Parameters[0].Type, lambda.Body.Type)
            .Invoke(this, new object[] { lambda });

        string tableName = sql[(sql.LastIndexOf("FROM ") + 5)..];
        tableName = tableName[..tableName.IndexOf(" AS ")];
        var before = $"UPDATE {tableName} SET ({columnName}) = (SELECT \"Value\" FROM (";
        var after = ") AS tttt WHERE tttt.\"Id\" = id)";

        return string.Join(Environment.NewLine, before, sql, after);
    }           

    public string ValidateFilter()
    {
        var (_, lambdaErrors) = _expressionScope.CovertFilterToLambda(_anotherSourceId, _sourcePairId); 

        return lambdaErrors;
    }
}
