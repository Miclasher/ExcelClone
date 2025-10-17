namespace ExcelClone.Domain.ExpressionLogic.AstNodes;

public class ValueNode : AstNode
{
    public ValueNode(object value)
    {
        Value = value;
    }

    public object Value { get; }
}