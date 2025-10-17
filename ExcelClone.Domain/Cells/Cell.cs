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
        Value = new CellValue(0m);
    }

    public Address Address { get; private set; }
    public string RawExpression {get; private set; }
    public CellValue Value { get; private set; }

    public void SetExpression(string expression, Parser parser)
    {
        ArgumentNullException.ThrowIfNull(expression);

        RawExpression = expression;
        _astNode = parser.Parse(expression);
    }

    public void Recalculate(Table context)
    {
        if (_astNode is null)
        {
            Value = new CellValue(RawExpression);
            return;
        }

        var evaluator = new 
    }
}