using System.Linq.Expressions;

namespace AegisForge.Infrastructure.Extension;

public class BasicExpressionVisitor(
    Expression oldValue,
    Expression newValue)
    : ExpressionVisitor
{
    public override Expression? Visit(Expression? node)
    {
        return node == oldValue ? newValue : base.Visit(node);
    }
}