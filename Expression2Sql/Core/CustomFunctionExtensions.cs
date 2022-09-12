using System;
using System.Linq.Expressions;
using System.Reflection;

namespace QueryingCore.Core;

public static class CustomFunctionExtensions
{
    public static bool any_null(params object?[] values) => throw new NotSupportedException("");

    public static bool eq(object? value1, object? value2)  => throw new NotSupportedException("");

    public static bool not_eq(object value1, object value2)  => throw new NotSupportedException("");

    public static DateTime? date_or_null(DateTime? value)  => throw new NotSupportedException("");

    public static DateTime today()  => throw new NotSupportedException("");
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
