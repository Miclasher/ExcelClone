using ExcelClone.Domain.Cells;

namespace ExcelClone.Domain.ExpressionLogic.AstNodes;

public class CellReferenceNode : AstNode
{
    public CellReferenceNode(Address cellAddress)
    {
        CellAddress = cellAddress;
    }

    public Address CellAddress { get; }
}