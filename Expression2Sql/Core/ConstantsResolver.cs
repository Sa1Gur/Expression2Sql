using System;
using System.Linq;
using System.Linq.Expressions;

namespace QueryingCore.Core
{
    public class ConstantsResolver : ExpressionVisitor
    {
        readonly Type[] _types =
        {
            typeof(Guid), typeof(DateTime), typeof(string), typeof(int), typeof(Guid?), typeof(DateTime?), typeof(int?)
        };

        protected override Expression VisitMember(MemberExpression node)
        {
            if (!(IsFromConstant(node) && _types.Contains(node.Type))) return node;
            
            object val = Expression.Lambda<Func<object>>(Expression.Convert(node, typeof(object))).Compile().Invoke();
            return Expression.Constant(val, node.Type);
        }

        static bool IsFromConstant(MemberExpression node)
        {
            if (IsStaticContextMember(node)) return false;

            return node.Expression.NodeType switch
            {
                ExpressionType.Constant => true,
                ExpressionType.MemberAccess => IsFromConstant((MemberExpression) node.Expression),
                _ => false
            };
        }

        static bool IsStaticContextMember(MemberExpression node) => node.Expression == null;
    }
}
