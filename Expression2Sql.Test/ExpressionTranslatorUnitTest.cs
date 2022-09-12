using System.Linq.Expressions;

namespace ExpressionTranslator.Test;

public class ExpressionTest
{
    [Fact]
    public void Test1()
    {
        string input = "";
        ExpressionCoverter expressionCoverter = new ();
        Expression expression = expressionCoverter.Convert(input);
    }
}
