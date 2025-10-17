namespace ExcelClone.Domain.Cells;

public sealed record Address(string Value)
{
    public static implicit operator Address(string value)
    {
        return new Address(value);
    }
};