using Antlr4.Runtime;
using Antlr4.Runtime.Tree;

namespace ExcelClone.Domain.ExpressionLogic;

public class AntlrParser
{
    public (IParseTree Tree, HashSet<string> Dependencies) Parse(string expression)
    {
        if (string.IsNullOrWhiteSpace(expression))
            return (new ValueNode(string.Empty), new HashSet<string>());

        if (!expression.StartsWith("="))
            return (ParseAsLiteral(expression), new HashSet<string>());

        var inputStream = new AntlrInputStream(expression.Substring(1));
        var lexer = new LabCalculatorLexer(inputStream);
        lexer.RemoveErrorListeners();
        lexer.AddErrorListener(ThrowExceptionErrorListener.Instance);

        var tokenStream = new CommonTokenStream(lexer);
        var parser = new LabCalculatorParser(tokenStream);
        parser.RemoveErrorListeners();
        parser.AddErrorListener(ThrowExceptionErrorListener.Instance);

        var tree = parser.compileUnit();

        var depVisitor = new DependencyVisitor();
        depVisitor.Visit(tree);

        return (tree, depVisitor.Dependencies);
    }

    private IParseTree ParseAsLiteral(string term)
    {
        if (decimal.TryParse(term, out var decVal))
            return new ValueNode(decVal);
        if (bool.TryParse(term, out var boolVal))
            return new ValueNode(boolVal);
        return new ValueNode(term);
    }
}