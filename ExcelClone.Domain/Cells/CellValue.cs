using System.Diagnostics;

namespace ExcelClone.Domain.Cells;

public readonly struct CellValue
{
    public CellValueType Type { get; private init; }

    private readonly bool _boolValue;
    private readonly decimal _decimalValue;
    private readonly string _stringValue = string.Empty;

    public CellValue(bool value)
    {
        Type = CellValueType.Bool;
        _boolValue = value;
    }

    public CellValue(decimal value)
    {
        Type = CellValueType.Decimal;
        _decimalValue = value;
    }

    public CellValue(string value)
    {
        Type = CellValueType.String;
        _stringValue = value;
    }

    public object GetValue()
    {
        return Type switch
        {
            CellValueType.Bool => _boolValue,
            CellValueType.Decimal => _decimalValue,
            CellValueType.String => _stringValue,
            _ => throw new UnreachableException("Tried to get value of cell with unknown value type")
        };
    }

    public bool TryGetDecimal(out decimal value)
    {
        if (Type == CellValueType.Decimal)
        {
            value = _decimalValue;
            return true;
        }

        value = _decimalValue;
        return false;
    }

    public override string ToString()
    {
        return Type switch
        {
            CellValueType.Decimal => _decimalValue.ToString(System.Globalization.CultureInfo.InvariantCulture),
            CellValueType.Bool => _boolValue ? "TRUE" : "FALSE",
            CellValueType.String => _stringValue,
            _ => string.Empty
        };
    }

    public bool TryGetBool(out bool cellValue)
    {
        if (Type == CellValueType.Bool)
        {
            cellValue = _boolValue;
            return true;
        }

        cellValue = _boolValue;
        return false;
    }
}