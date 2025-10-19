using ExcelClone.Domain.ExpressionLogic;
using ExcelClone.Domain.ExpressionLogic.AstNodes;
namespace ExcelClone.Tests;

[TestClass]
public class ParserTests
{
    [TestMethod]
    public void SimpleMultiplicationParserTest()
    {
        var parser = new Parser();
            
        var result = parser.Parse("=123 * 2");
            
        Assert.IsInstanceOfType(result, typeof(BinaryOperationNode));
        var binaryNode = (BinaryOperationNode)result;
        Assert.AreEqual("*", binaryNode.Op);
            
        Assert.IsInstanceOfType(binaryNode.Left, typeof(ValueNode));
        Assert.IsInstanceOfType(binaryNode.Right, typeof(ValueNode));
            
        var leftValue = ((ValueNode)binaryNode.Left).Value;
        var rightValue = ((ValueNode)binaryNode.Right).Value;
            
        Assert.AreEqual(123m, leftValue);
        Assert.AreEqual(2m, rightValue);
    }
}
