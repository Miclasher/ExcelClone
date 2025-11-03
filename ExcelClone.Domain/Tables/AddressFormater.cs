using static System.Text.RegularExpressions.Regex;

namespace ExcelClone.Domain.Tables;

public static class AddressFormater
{
    public static (int col, int row) ParseAddress(string address)
    {
        var match = Match(address.ToUpper(), @"([A-Z]+)(\d+)");
        if (!match.Success)
        {
            return (-1, -1);
        }

        var colStr = match.Groups[1].Value;
        var row = int.Parse(match.Groups[2].Value) - 1;

        var col = 0;
        foreach (var t in colStr)
        {
            col = col * 26 + (t - 'A' + 1);
        }
        return (col - 1, row);
    }

    public static string FormatAddress(int col, int row)
    {
        if (col < 0 || row < 0)
        {
            return "#REF!";
        }

        var colStr = "";
        var c = col + 1;
        while (c > 0)
        {
            var m = (c - 1) % 26;
            colStr = (char)('A' + m) + colStr;
            c = (c - m) / 26;
        }
        return $"{colStr}{row + 1}";
    }
}
