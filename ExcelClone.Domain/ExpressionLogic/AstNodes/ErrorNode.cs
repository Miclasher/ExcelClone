namespace ExcelClone.Domain.ExpressionLogic.AstNodes;

public class ErrorNode : AstNode
{
    public ErrorNode(string message)
    {
        Message = message;
    }

    public string Message { get; }
}