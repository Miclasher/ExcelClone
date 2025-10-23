using Antlr4.Runtime;

namespace ExcelClone.Domain.ExpressionLogic;

public class ThrowExceptionErrorListener : BaseErrorListener, IAntlrErrorListener<int>
{
    public static readonly ThrowExceptionErrorListener Instance = new();

    private ThrowExceptionErrorListener()
    {

    }

    private const string IncorrectExpression = "Некоректний вираз: {0}";

    public override void SyntaxError(IRecognizer recognizer, IToken offendingSymbol, int line, int charPositionInLine, string msg, RecognitionException e)
    {
        throw new ArgumentException(IncorrectExpression, msg, e);
    }

    public void SyntaxError(IRecognizer recognizer, int offendingSymbol, int line, int charPositionInLine, string msg, RecognitionException e)
    {
        throw new ArgumentException(IncorrectExpression, msg, e);
    }
}