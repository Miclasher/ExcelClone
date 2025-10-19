using System.Text.RegularExpressions;

namespace ExcelClone.Domain.ExpressionLogic.Tokens;

public static class Tokenizer
{
    private record TokenDefinition(TokenType Type, Regex Regex);
    private static readonly List<TokenDefinition> Definitions =
    [
        new(TokenType.Number, new Regex(@"^\d+(\.\d+)?")),
        new(TokenType.Boolean, new Regex(@"^(?i)(true|false)\b")),
        new(TokenType.Identifier, new Regex(@"^[a-zA-Z_][a-zA-Z0-9_]*")),
        new(TokenType.Plus, new Regex(@"^\+")),
        new(TokenType.Minus, new Regex(@"^-")),
        new(TokenType.Star, new Regex(@"^\*")),
        new(TokenType.Slash, new Regex(@"^/")),
        new(TokenType.Mod, new Regex(@"^(?i)mod\b")),
        new(TokenType.Div, new Regex(@"^(?i)div\b")),
        new(TokenType.LessOrEqual, new Regex(@"^<=")),
        new(TokenType.GreaterOrEqual, new Regex(@"^>=")),
        new(TokenType.NotEqual, new Regex(@"^<>")),
        new(TokenType.Equal, new Regex(@"^=")),
        new(TokenType.Less, new Regex(@"^<")),
        new(TokenType.Greater, new Regex(@"^>")),
        new(TokenType.LeftParen, new Regex(@"^\(")),
        new(TokenType.RightParen, new Regex(@"^\)")),
        new(TokenType.Comma, new Regex(@"^,"))
    ];

    public static List<Token> Tokenize(string source)
    {
        var tokens = new List<Token>();
        var position = 0;
        while (position < source.Length)
        {
            if (char.IsWhiteSpace(source[position])) { position++; continue; }

            var matchFound = false;
            foreach (var def in Definitions)
            {
                var match = def.Regex.Match(source[position..]);
                if (match.Success)
                {
                    object? literal = def.Type switch
                    {
                        TokenType.Number => decimal.Parse(match.Value),
                        TokenType.Boolean => bool.Parse(match.Value.ToLower()),
                        _ => null
                    };
                    tokens.Add(new Token(def.Type, match.Value, literal, position));
                    position += match.Length;
                    matchFound = true;
                    break;
                }
            }
            if (!matchFound)
            {
                throw new Exception($"Unexpected character at position {position}");
            }
        }
        tokens.Add(new Token(TokenType.Eof, "", null, position));
        return tokens;
    }
}
