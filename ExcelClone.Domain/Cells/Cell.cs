using System.ComponentModel.DataAnnotations.Schema;
using ExcelClone.Domain.ExpressionLogic;
using ExcelClone.Domain.Tables;

namespace ExcelClone.Domain.Cells;

public sealed class Cell
{
    private AstNode? _astNode;

    public Cell(Address address)
    {
        Address = address;
        RawExpression = string.Empty;
        CellValue = new CellValue(0m);
    }

    public Address Address { get; private set; }
    public string RawExpression {get; private set; }
    public CellValue CellValue { get; private set; }

    public void Recalculate(Table context)
    {
        throw new NotImplementedException();
    }
}