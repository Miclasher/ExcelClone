using ExcelClone.Domain.Cells;
using ExcelClone.Domain.ExpressionLogic.AstNodes;
using ExcelClone.Domain.Tables;

namespace ExcelClone.Domain.ExpressionLogic;

public class Evaluator
{
    private readonly Table _context;

    public Evaluator(Table context)
    {
        _context = context;
    }

    public CellValue Evaluate(AstNode node)
    {
        throw new NotImplementedException();
    }
}