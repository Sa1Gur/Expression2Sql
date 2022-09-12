using System;
using System.Linq.Expressions;

namespace QueryingCore.Core;

public class RRExpressionVisitor<TSrc, TDst> : ExpressionVisitor
{
    readonly Expression<Func<TSrc, TDst>> _one;

    readonly Expression<Func<TSrc, TDst>> _another;

    public RRExpressionVisitor(Expression<Func<TSrc, TDst>> one, Expression<Func<TSrc, TDst>> another) => (_one, _another) = (one, another);

    protected override Expression VisitMember(MemberExpression node)
    {
        if (node.Expression != null && node.Expression.Type == typeof(TSrc))
        {
            var nd = _one.Body.Replace(_one.Parameters[0], node.Expression);
            if (node.ToString() == nd.ToString())
            {
                var exp = _another.Body.Replace(_another.Parameters[0], node.Expression);
                return exp;
            }
        }

        return base.VisitMember(node);
    }
}
