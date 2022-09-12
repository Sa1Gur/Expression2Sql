using System;
using System.Linq.Expressions;
using System.Reflection;

namespace QueryingCore.Core;

public interface IQueryingContext
{
    LambdaExpression Evaluate(Type typeWrapper);
}

public class QueryingContext<TMain> : IQueryingContext where TMain : RecordBase
{
    public LambdaExpression Evaluate(Type typeWrapper)
    {
        return (LambdaExpression)CSharpScriptEvaluate(typeWrapper);
    }

    private static Expression Unbox(Expression expression)
    {
        while (true)
        {
            if (expression.NodeType == ExpressionType.Convert)
            {
                expression = ((UnaryExpression) expression).Operand;
                continue;
            }

            return expression;
        }
    }

    public static Expression CSharpScriptEvaluate(Type typeWrapper, string expressionName = "expr")
    {
        var exprField = typeWrapper.GetProperty(expressionName, BindingFlags.Public | BindingFlags.Instance);
        var main = Activator.CreateInstance(typeWrapper);

        var expressionBoxed = exprField.GetValue(main) as LambdaExpression;

        var expressionUnboxed = Unbox(expressionBoxed.Body);

        var sInstance = (ExpressionExtensions.IS) Activator.CreateInstance(
            typeof(ExpressionExtensions.S<,>).MakeGenericType(typeof(TMain), expressionUnboxed.Type));
        return sInstance.Create(expressionUnboxed, expressionBoxed.Parameters[0]);
    }
}
