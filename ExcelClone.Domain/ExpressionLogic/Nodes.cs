using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;

namespace ExcelClone.Domain.ExpressionLogic;


public abstract class CustomParseNode : IParseTree
{
    ITree ITree.GetChild(int i)
    {
        return GetChild(i);
    }

    public string ToStringTree()
    {
        throw new NotImplementedException();
    }

    ITree ITree.Parent { get; }
    public IParseTree Parent { get; set; } = null!;
    public object Payload { get; set; } = null!;
    public int ChildCount => 0;
    public IParseTree GetChild(int i) => throw new NotSupportedException();
    public T Accept<T>(IParseTreeVisitor<T> visitor) => visitor.Visit(this);
    public string GetText() => ToString()!;
    public string ToStringTree(Antlr4.Runtime.Parser parser) => ToString()!;
    public Interval SourceInterval { get; }
}

public class ValueNode(object value) : CustomParseNode
{
    public object Value { get; } = value;
    public override string ToString() => Value?.ToString() ?? "";
}

public class ErrorNode(string message) : CustomParseNode
{
    public string Message { get; } = message;
    public override string ToString() => Message;
}