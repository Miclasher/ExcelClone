using ExcelClone.Domain.ExpressionLogic.AstNodes;

namespace ExcelClone.Domain.Tables;
public sealed class AstStringBuilder
{
    public string Build(AstNode node)
    {
        return node switch
        {
            ValueNode { Value: decimal or bool } vn => vn.Value.ToString()!,
            ValueNode { Value: string } vn2 => (string)vn2.Value,
            _ => "=" + BuildExpression(node)
        };
    }

    private string BuildExpression(AstNode node)
    {
        return node switch
        {
            ValueNode vn => vn.Value?.ToString() ?? "",
            CellReferenceNode crn => crn.CellAddress,
            BinaryOperationNode bon => $"({BuildExpression(bon.Left)} {bon.Op} {BuildExpression(bon.Right)})",
            FunctionCallNode fcn => $"{fcn.FunctionName}({string.Join(", ", fcn.Arguments.Select(BuildExpression))})",
            ErrorNode en => en.Message,
            _ => ""
        };
    }
}