namespace ExcelClone.Domain.ExpressionLogic.AstNodes;

public class CellReferenceNode : AstNode
{
    public CellReferenceNode(string cellAddress)
    {
        CellAddress = cellAddress;
    }

    public string CellAddress { get; }
}