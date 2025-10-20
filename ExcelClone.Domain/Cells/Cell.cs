using System.ComponentModel.DataAnnotations.Schema;
using ExcelClone.Domain.ExpressionLogic;
using ExcelClone.Domain.ExpressionLogic.AstNodes;
using ExcelClone.Domain.Tables;

namespace ExcelClone.Domain.Cells;

public sealed class Cell
{
    private AstNode? _astNode;
    
    public Cell(string address)
    {
        Address = address;
        RawExpression = string.Empty;
        Value = new CellValue(string.Empty);
        Dependencies = [];
    }

    public string Address { get; private set; }
    public string RawExpression {get; private set; }
    public CellValue Value { get; set; }
    
    public HashSet<string> Dependencies { get; private set; }

    public void SetExpression(string expression, Parser parser)
    {
        ArgumentNullException.ThrowIfNull(expression);

        RawExpression = expression;
        _astNode = parser.Parse(expression);
        
        Dependencies.Clear();
        
        if (_astNode != null)
        {
            ExtractDependencies(_astNode);
        }
    }
    
    private void ExtractDependencies(AstNode node)
    {
        switch (node)
        {
            case CellReferenceNode refNode:
                Dependencies.Add(refNode.CellAddress);
                break;
                
            case BinaryOperationNode binNode:
                ExtractDependencies(binNode.Left);
                ExtractDependencies(binNode.Right);
                break;
                
            case FunctionCallNode funcNode:
                foreach (var arg in funcNode.Arguments)
                {
                    ExtractDependencies(arg);
                }
                break;
        }
    }

    public void Recalculate(Table context)
    {
        if (_astNode is null)
        {
            return;
        }

        var evaluator = new Evaluator(context);
        Value = evaluator.Evaluate(_astNode);
    }
}