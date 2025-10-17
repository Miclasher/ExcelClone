namespace ExcelClone.Domain.ExpressionLogic.AstNodes;

public class BinaryOperationNode : AstNode
{
    public BinaryOperationNode(AstNode right, AstNode left, string op)
    {
        Right = right;
        Left = left;
        Op = op;
    }

    public AstNode Right { get; }
    public AstNode Left { get; }
    public string Op { get; }
}