namespace ExcelClone.Domain.ExpressionLogic.AstNodes;

public class FunctionCallNode : AstNode
{
    public FunctionCallNode(string functionName, List<AstNode> arguments)
    {
        FunctionName = functionName;
        Arguments = arguments;
    }

    public string FunctionName { get; }
    public List<AstNode> Arguments { get; }

}