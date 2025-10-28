using Antlr4.Runtime.Tree;
using ExcelClone.Domain.Cells;
using ExcelClone.Domain.Tables;
using System.Globalization;

namespace ExcelClone.Domain.ExpressionLogic;

public class CalculatorVisitor : LabCalculatorBaseVisitor<CellValue>
{
    private readonly Table _table;

    public CalculatorVisitor(Table table)
    {
        _table = table;
    }

    public override CellValue VisitCompileUnit(LabCalculatorParser.CompileUnitContext context)
    {
        return Visit(context.expression());
    }

    public override CellValue VisitParenthesizedExpr(LabCalculatorParser.ParenthesizedExprContext context)
    {
        return Visit(context.expression());
    }

    public override CellValue VisitNumberAtom(LabCalculatorParser.NumberAtomContext context)
    {
        var text = context.GetText().Replace(',', '.');
        if (decimal.TryParse(text, NumberStyles.Any, CultureInfo.InvariantCulture, out var result))
        {
            return new CellValue(result);
        }
        return new CellValue("#VALUE!");
    }

    public override CellValue VisitBooleanAtom(LabCalculatorParser.BooleanAtomContext context)
    {
        return new CellValue(bool.Parse(context.GetText().ToLower()));
    }

    public override CellValue VisitCellReferenceAtom(LabCalculatorParser.CellReferenceAtomContext context)
    {
        var address = context.GetText().ToUpper();
        if (_table.TryGetCell(address, out var cell))
        {
            return cell.Value;
        }
        return new CellValue("#REF!");
    }

    public override CellValue VisitUnaryExpr(LabCalculatorParser.UnaryExprContext context)
    {
        var value = Visit(context.expression());
        if (value.TryGetDecimal(out var decVal))
        {
            return new CellValue(-decVal);
        }
        return new CellValue("#VALUE!");
    }

    public override CellValue VisitAdditiveExpr(LabCalculatorParser.AdditiveExprContext context)
    {
        var (left, right, error) = EvaluateBinaryOperands(context.expression(0), context.expression(1));
        if (error != null) return error.Value;

        return context.op.Type == LabCalculatorLexer.ADD
            ? new CellValue(left + right)
            : new CellValue(left - right);
    }

    public override CellValue VisitMultiplicativeExpr(LabCalculatorParser.MultiplicativeExprContext context)
    {
        var (left, right, error) = EvaluateBinaryOperands(context.expression(0), context.expression(1));
        if (error != null) return error.Value;

        return context.op.Type switch
        {
            LabCalculatorLexer.MULTIPLY => new CellValue(left * right),
            LabCalculatorLexer.DIVIDE => right != 0 ? new CellValue(left / right) : new CellValue("#DIV/0!"),
            LabCalculatorLexer.MOD => right != 0 ? new CellValue(left % right) : new CellValue("#DIV/0!"),
            LabCalculatorLexer.DIV =>
                right != 0 ? new CellValue(Math.Truncate(left / right)) : new CellValue("#DIV/0!"),
            _ => new CellValue("#OP!")
        };
    }

    public override CellValue VisitComparisonExpr(LabCalculatorParser.ComparisonExprContext context)
    {
        var leftVal = Visit(context.expression(0));
        var rightVal = Visit(context.expression(1));

        if (leftVal.Type == CellValueType.String) return leftVal;
        if (rightVal.Type == CellValueType.String) return rightVal;

        // Comparing decimals
        if (leftVal.TryGetDecimal(out var leftDec) && rightVal.TryGetDecimal(out var rightDec))
        {
            return context.op.Type switch
            {
                LabCalculatorLexer.GT => new CellValue(leftDec > rightDec),
                LabCalculatorLexer.GTE => new CellValue(leftDec >= rightDec),
                LabCalculatorLexer.LT => new CellValue(leftDec < rightDec),
                LabCalculatorLexer.LTE => new CellValue(leftDec <= rightDec),
                LabCalculatorLexer.EQ => new CellValue(leftDec == rightDec),
                LabCalculatorLexer.NEQ => new CellValue(leftDec != rightDec),
                _ => new CellValue("#OP!")
            };
        }

        // Comparing booleans
        if (leftVal.TryGetBool(out var leftBool) && rightVal.TryGetBool(out var rightBool))
        {
            return context.op.Type switch
            {
                LabCalculatorLexer.EQ => new CellValue(leftBool == rightBool),
                LabCalculatorLexer.NEQ => new CellValue(leftBool != rightBool),
                _ => new CellValue("#VALUE!")
            };
        }

        return new CellValue("#VALUE!");
    }

    public override CellValue VisitMinMaxExpr(LabCalculatorParser.MinMaxExprContext context)
    {
        var args = context.expression().Select(Visit).ToList();
        var (decimals, error) = ExtractDecimals(args);

        if (error != null) return error.Value;
        if (decimals.Count == 0) return new CellValue("#N/A");

        return context.op.Type == LabCalculatorLexer.MAX
            ? new CellValue(decimals.Max())
            : new CellValue(decimals.Min());
    }

    public override CellValue Visit(IParseTree tree)
    {
        if (tree is ValueNode vn) return HandleValueNode(vn);
        if (tree is ErrorNode en) return new CellValue(en.Message);
        return base.Visit(tree);
    }

    public static CellValue HandleValueNode(ValueNode node) => node.Value switch
    {
        decimal d => new CellValue(d),
        bool b => new CellValue(b),
        string s => new CellValue(s),
        _ => new CellValue("#VALUE!")
    };

    private (decimal Left, decimal Right, CellValue? Error) EvaluateBinaryOperands(IParseTree leftCtx, IParseTree rightCtx)
    {
        var leftVal = Visit(leftCtx);
        var rightVal = Visit(rightCtx);

        if (leftVal.Type == CellValueType.String) return (0, 0, leftVal);
        if (rightVal.Type == CellValueType.String) return (0, 0, rightVal);

        if (leftVal.TryGetDecimal(out var leftDec) && rightVal.TryGetDecimal(out var rightDec))
        {
            return (leftDec, rightDec, null);
        }
        return (0, 0, new CellValue("#VALUE!"));
    }

    private static (List<decimal> Decimals, CellValue? Error) ExtractDecimals(List<CellValue> values)
    {
        var decimals = new List<decimal>();
        foreach (var val in values)
        {
            if (val.Type == CellValueType.String) return ([], val);
            if (!val.TryGetDecimal(out var decVal)) return ([], new CellValue("#VALUE!"));
            decimals.Add(decVal);
        }
        return (decimals, null);
    }
}

