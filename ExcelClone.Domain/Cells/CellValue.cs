using System.Diagnostics;
using ExcelClone.Domain.Abstractions;

namespace ExcelClone.Domain.Cells;

public readonly struct CellValue
{
    public CellValueType Type { get; private init; }

    private readonly bool _boolValue;
    private readonly decimal _decimalValue;
    private readonly Error _errorValue;

    public CellValue(bool value)
    {
        _boolValue = value;
        _errorValue = Error.None();
    }

    public CellValue(decimal value)
    {
        _decimalValue = value;
        _errorValue = Error.None();
    }

    public CellValue(Error value)
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