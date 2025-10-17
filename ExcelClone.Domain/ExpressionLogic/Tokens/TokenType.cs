namespace ExcelClone.Domain.ExpressionLogic.Tokens;

public enum TokenType
{
    Plus, Minus, Star, Slash,
    Equal, NotEqual, Greater, GreaterOrEqual, Less, LessOrEqual,
    Mod, Div,
    LeftParen, RightParen, Comma,
    Number, Identifier, Boolean,
    Eof
}