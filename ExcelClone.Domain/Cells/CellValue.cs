using System.Diagnostics;

namespace ExcelClone.Domain.Cells;

public readonly struct CellValue
{
    public CellValueType Type { get; private init; }

    private readonly bool _boolValue;
    private readonly decimal _decimalValue;
    private readonly string _errorValue = string.Empty;

    public CellValue(bool value)
    {
        _boolValue = value;
    }

    public CellValue(decimal value)
    {
        _decimalValue = value;
    }

    public CellValue(string value)
    {
        _errorValue = value;
    }

    public object GetValue()
    {
        return Type switch
        {
            CellValueType.Bool => _boolValue,
            CellValueType.Decimal => _decimalValue,
            CellValueType.Error => _errorValue,
            _ => throw new UnreachableException("Tried to get value of cell with unknown value type")
        };
    }
}