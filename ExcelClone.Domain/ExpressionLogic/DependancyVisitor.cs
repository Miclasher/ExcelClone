namespace ExcelClone.Domain.ExpressionLogic;

public class DependencyVisitor : LabCalculatorBaseVisitor<object>
{
    public HashSet<string> Dependencies { get; } = new();

    public override object VisitCellReferenceAtom(LabCalculatorParser.CellReferenceAtomContext context)
    {
        Dependencies.Add(context.GetText().ToUpper());
        return null!;
    }
}