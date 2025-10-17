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

    public CellValue Evaluate(AstNode node) => node switch
    {
        ValueNode vn => HandleValueNode(vn),
        CellReferenceNode crn => _context.GetCell(crn.CellAddress).Value,
        BinaryOperationNode bon => PerformBinaryOperation(bon),
        FunctionCallNode fcn => PerformFunctionCall(fcn),
        ErrorNode en => new CellValue(en.Message),
        _ => new CellValue("#EVAL!")
    };

    private static CellValue HandleValueNode(ValueNode node) => node.Value switch
    {
        decimal d => new CellValue(d),
        bool b => new CellValue(b),
        string s => new CellValue(s),
        _ => new CellValue("#VALUE!")
    };

    private CellValue PerformFunctionCall(FunctionCallNode fcn)
    { 
        var evaluatedArgs = fcn.Arguments.Select(Evaluate).ToList();
        
        var firstError = evaluatedArgs.FirstOrDefault(arg => arg.Type == CellValueType.String);
        if (firstError.Type == CellValueType.String) return firstError;

        var decimalArgs = new List<decimal>();
        foreach (var arg in evaluatedArgs)
        {
            if (arg.TryGetDecimal(out var decVal))
            {
                decimalArgs.Add(decVal);
            }
            else return new CellValue("#VALUE!");
        }

        if (decimalArgs.Count == 0) return new CellValue("#N/A");

        return fcn.FunctionName switch
        {
            "mmax" => new CellValue(decimalArgs.Max()),
            "mmin" => new CellValue(decimalArgs.Min()),
            _ => new CellValue("#NAME?")
        };
    }

    private CellValue PerformBinaryOperation(BinaryOperationNode opNode)
    {
        var left = Evaluate(opNode.Left);
        var right = Evaluate(opNode.Right);

        if (left.Type == CellValueType.String) return left;
        if (right.Type == CellValueType.String) return right;

        if (!left.TryGetDecimal(out var leftDec) || !right.TryGetDecimal(out var rightDec))
        {
            return new CellValue("#VALUE!");
        }

        return opNode.Op switch
        {
            "+" => new CellValue(leftDec + rightDec),
            "-" => new CellValue(leftDec - rightDec),
            "*" => new CellValue(leftDec * rightDec),
            "/" => rightDec != 0 ? new CellValue(leftDec / rightDec) : new CellValue("#DIV/0!"),
            "mod" => rightDec != 0 ? new CellValue(leftDec % rightDec) : new CellValue("#DIV/0!"),
            "div" => rightDec != 0 ? new CellValue(Math.Truncate(leftDec / rightDec)) : new CellValue("#DIV/0!"),
            ">" => new CellValue(leftDec > rightDec),
            "<" => new CellValue(leftDec < rightDec),
            "=" => new CellValue(leftDec == rightDec),
            ">=" => new CellValue(leftDec >= rightDec),
            "<=" => new CellValue(leftDec <= rightDec),
            "<>" => new CellValue(leftDec != rightDec),
            _ => new CellValue("#OP!")
        };
    }
}