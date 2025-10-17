namespace ExcelClone.Domain.ExpressionLogic.Tokens;

public sealed record Token(TokenType Type, string Value, object? Literal, int Position);