using ExcelClone.Domain.Cells;
using ExcelClone.Domain.ExpressionLogic.AstNodes;
using ExcelClone.Domain.ExpressionLogic.Tokens;

namespace ExcelClone.Domain.ExpressionLogic;

public sealed class Parser
{
    private List<Token> _tokens = [];
    private int _position;

    public AstNode Parse(string expression)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(expression))
            {
                return new ValueNode(string.Empty);
            }

            if (!expression.StartsWith('='))
            {
                return ParseAsLiteral(expression);
            }

            _tokens = Tokenizer.Tokenize(expression[1..]);
            _position = 0;

            if (IsAtEnd())
            {
                return new ValueNode(string.Empty);
            }

            var result = ParseExpression();

            if (!IsAtEnd())
            {
                return new ErrorNode("#SYNTAX!");
            }

            return result;
        }
        catch (Exception)
        {
            return new ErrorNode("#SYNTAX!");
        }
    }

    private AstNode ParseExpression()
    {
        var left = ParseTerm();
        while (Match(TokenType.Plus, TokenType.Minus, TokenType.Equal, TokenType.NotEqual,
                       TokenType.Less, TokenType.LessOrEqual, TokenType.Greater, TokenType.GreaterOrEqual))
        {
            var op = Previous().Value;
            var right = ParseTerm();
            left = new BinaryOperationNode(right, left, op);
        }
        return left;
    }

    private AstNode ParseTerm()
    {
        var left = ParseFactor();
        while (Match(TokenType.Star, TokenType.Slash, TokenType.Mod, TokenType.Div))
        {
            var op = Previous().Value;
            var right = ParseFactor();
            left = new BinaryOperationNode(right, left, op);
        }
        return left;
    }

    private AstNode ParseFactor()
    {
        if (Match(TokenType.Number, TokenType.Boolean))
        {
            return new ValueNode(Previous().Literal!);
        }


        if (Match(TokenType.Identifier))
        {
            var identifier = Previous().Value;
            if (Match(TokenType.LeftParen))
            {
                var args = new List<AstNode>();
                if (!Check(TokenType.RightParen))
                {
                    do { args.Add(ParseExpression()); }
                    while (Match(TokenType.Comma));
                }
                Consume(TokenType.RightParen, "Expected ')' after arguments.");
                return new FunctionCallNode(identifier.ToLower(), args);
            }
            return new CellReferenceNode(identifier);
        }
        if (Match(TokenType.LeftParen))
        {
            var expr = ParseExpression();
            Consume(TokenType.RightParen, "Expected ')' after expression.");
            return expr;
        }
        throw new Exception("Parse error: expected expression.");
    }

    private bool Match(params TokenType[] types)
    {
        foreach (var type in types)
        {
            if (Check(type)) { Advance(); return true; }
        }
        return false;
    }
    private Token Consume(TokenType type, string message)
    {
        if (Check(type))
        {
            return Advance();
        }

        throw new Exception(message);
    }
    private bool Check(TokenType type) => !IsAtEnd() && Peek().Type == type;
    private Token Advance() { if (!IsAtEnd())
        {
            _position++;
        }

        return Previous(); }
    private bool IsAtEnd() => Peek().Type == TokenType.Eof;
    private Token Peek() => _tokens[_position];
    private Token Previous() => _tokens[_position - 1];
    private ValueNode ParseAsLiteral(string term)
    {
        if (decimal.TryParse(term, out var decimalValue))
        {
            return new ValueNode(decimalValue);
        }
        
        if (bool.TryParse(term, out var boolVal))
        {
            return new ValueNode(boolVal);
        }

        return new ValueNode(term);
    }
}
