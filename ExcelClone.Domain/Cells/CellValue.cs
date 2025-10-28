using System.Diagnostics;
using System.Text.Json.Serialization;

namespace ExcelClone.Domain.Cells;

public readonly struct CellValue
{
    [JsonPropertyName("type")]
    public CellValueType Type { get; }

    [JsonInclude]
    [JsonPropertyName("boolvalue")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    private bool BoolValue { get; init; }

    [JsonInclude]
    [JsonPropertyName("decimalvalue")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    private decimal DecimalValue { get; init; }

    [JsonInclude]
    [JsonPropertyName("stringvalue")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    private string? StringValue { get; init; }

    public CellValue(bool value)
    {
        Type = CellValueType.Bool;
        BoolValue = value;
    }

    public CellValue(decimal value)
    {
        Type = CellValueType.Decimal;
        DecimalValue = value;
    }

    public CellValue(string value)
    {
        Type = CellValueType.String;
        StringValue = value;
    }

    [JsonConstructor]
    public CellValue(CellValueType type, bool boolvalue, decimal decimalvalue, string? stringvalue)
    {
        Type = type;
        BoolValue = boolvalue;
        DecimalValue = decimalvalue;
        StringValue = stringvalue;
    }

    public object? GetValue()
    {
        return Type switch
        {
            CellValueType.Bool => BoolValue,
            CellValueType.Decimal => DecimalValue,
            CellValueType.String => StringValue,
            _ => throw new UnreachableException("Tried to get value of cell with unknown value type")
        };
    }

    public bool TryGetDecimal(out decimal value)
    {
        if (Type == CellValueType.Decimal)
        {
            value = DecimalValue;
            return true;
        }

        value = DecimalValue;
        return false;
    }

    public override string? ToString()
    {
        return Type switch
        {
            CellValueType.Decimal => DecimalValue.ToString(System.Globalization.CultureInfo.InvariantCulture),
            CellValueType.Bool => BoolValue ? "TRUE" : "FALSE",
            CellValueType.String => StringValue,
            _ => string.Empty
        };
    }

    public bool TryGetBool(out bool cellValue)
    {
        if (Type == CellValueType.Bool)
        {
            cellValue = BoolValue;
            return true;
        }

        cellValue = BoolValue;
        return false;
    }
}