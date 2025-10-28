using System.Text.Json.Serialization;
using Antlr4.Runtime.Tree;
using ExcelClone.Domain.ExpressionLogic;
using ExcelClone.Domain.Tables;

namespace ExcelClone.Domain.Cells;

public class Cell
{
    public string Address { get; }
    public string RawExpression { get; private set; } = string.Empty;
    public CellValue Value { get; set; } = new(string.Empty);

    private IParseTree? _antlrTree;
    public HashSet<string> Dependencies { get; private set; } = new();

    public Cell(string address) => Address = address;

    [JsonConstructor]
    public Cell(string address, string rawExpression, CellValue value, HashSet<string> dependencies) : this(address)
    {
        RawExpression = rawExpression;
        Value = value;
        Dependencies = dependencies;
    }

    public void SetExpression(string expression, AntlrParser parser)
    {
        RawExpression = expression;

        try
        {
            var (tree, dependencies) = parser.Parse(expression);
            _antlrTree = tree;
            Dependencies = dependencies;
        }
        catch (Exception)
        {
            _antlrTree = new ErrorNode("#SYNTAX!");
            Dependencies.Clear();
        }
    }

    public void Recalculate(Table context)
    {
        if (_antlrTree == null)
        {
            Value = new CellValue(RawExpression);
            return;
        }

        try
        {
            var evaluator = new CalculatorVisitor(context);
            Value = evaluator.Visit(_antlrTree);
        }
        catch (Exception)
        {
            Value = new CellValue("#EVAL!");
        }
    }
}