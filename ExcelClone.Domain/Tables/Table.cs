using ExcelClone.Domain.Cells;
using ExcelClone.Domain.ExpressionLogic;
using System.Collections.Concurrent;
using System.Text.Json.Serialization;
using static ExcelClone.Domain.Tables.AddressFormater;

namespace ExcelClone.Domain.Tables;

public class Table
{
    public string Id { get; private init; }

    [JsonInclude]
    [JsonPropertyName("cells")]
    private ConcurrentDictionary<string, Cell> Cells { get; init; } = [];

    private readonly AntlrParser _parser = new();

    private readonly ConcurrentDictionary<string, HashSet<string>> _dependents = new();

    public Table(string id)
    {
        Id = id;
    }

    [JsonConstructor]
    public Table(string id, ConcurrentDictionary<string, Cell>? cells)
    {
        Id = id;
        Cells = cells ?? new ConcurrentDictionary<string, Cell>();
        RebuildDependents();
    }

    public (int MaxCol, int MaxRow) GetDimensions()
    {
        var maxRow = -1;
        var maxCol = -1;

        if (Cells.IsEmpty)
        {
            return (9, 19);
        }

        foreach (var address in Cells.Keys)
        {
            var (col, row) = ParseAddress(address);
            if (row > maxRow) maxRow = row;
            if (col > maxCol) maxCol = col;
        }

        return (Math.Max(maxCol, 9), Math.Max(maxRow, 19));
    }

    public void RemoveLastRow()
    {
        var (maxCol, maxRow) = GetDimensions();
        if (maxRow == 0)
        {
            return;
        }

        for (var c = 0; c <= maxCol; c++)
        {
            var address = FormatAddress(c, maxRow);
            if (Cells.TryRemove(address, out var cell))
            {
                ClearCellDependencies(cell, address);
            }
        }
        RecalculateAll();
    }

    public void RemoveLastColumn()
    {
        var (maxCol, maxRow) = GetDimensions();
        if (maxCol == 0)
        {
            return;
        }

        for (var r = 0; r <= maxRow; r++)
        {
            var address = FormatAddress(maxCol, r);
            if (Cells.TryRemove(address, out var cell))
            {
                ClearCellDependencies(cell, address);
            }
        }
        RecalculateAll();
    }

    public ICollection<Cell> GetAllCells()
    {
        return Cells.Values;
    }

    public Cell GetOrAddCell(string address)
    {
        return Cells.GetOrAdd(address, addressOfNewCell => new Cell(addressOfNewCell));
    }

    public bool TryGetCell(string address, out Cell o)
    {
        return Cells.TryGetValue(address, out o!);
    }

    public void AddColumn()
    {
        var (maxCol, _) = GetDimensions();
        var newAddress = FormatAddress(maxCol + 1, 0);
        GetOrAddCell(newAddress);
    }

    public void AddRow()
    {
        var (_, maxRow) = GetDimensions();
        var newAddress = FormatAddress(0, maxRow + 1);
        GetOrAddCell(newAddress);
    }

    public void SetExpression(string address, string expression)
    {
        ArgumentNullException.ThrowIfNull(expression);
        ArgumentNullException.ThrowIfNull(address);

        var cell = GetOrAddCell(address);

        var oldDependencies = new HashSet<string>(cell.Dependencies);

        cell.SetExpression(expression, _parser);

        foreach (var oldDep in oldDependencies)
        {
            if (_dependents.TryGetValue(oldDep, out var dependents))
            {
                dependents.Remove(address);
            }
        }

        foreach (var newDep in cell.Dependencies)
        {
            var dependentsList = _dependents.GetOrAdd(newDep, _ => new HashSet<string>());
            dependentsList.Add(address);
        }

        RecalculateAll();
    }

    private void RecalculateAll()
    {
        var calculationOrder = GetCalculationOrder();

        foreach (var address in calculationOrder)
        {
            if (Cells.TryGetValue(address, out var cell))
            {
                cell.Recalculate(this);
            }
        }
    }

    private List<string> GetCalculationOrder()
    {
        var result = new List<string>();
        var visited = new HashSet<string>();
        var temporaryMark = new HashSet<string>();

        foreach (var address in Cells.Keys)
        {
            if (!visited.Contains(address))
            {
                if (!Visit(address, visited, temporaryMark, result))
                {
                    MarkCircularReferences(temporaryMark);
                    temporaryMark.Clear();
                }
            }
        }

        return result;
    }

    private bool Visit(string address, HashSet<string> visited, HashSet<string> temporaryMark, List<string> result)
    {
        if (temporaryMark.Contains(address))
        {
            return false;
        }

        if (visited.Contains(address))
        {
            return true;
        }

        temporaryMark.Add(address);

        if (Cells.TryGetValue(address, out var cell))
        {
            if (cell.Dependencies.Any(dependency => !Visit(dependency, visited, temporaryMark, result)))
            {
                return false;
            }
        }

        temporaryMark.Remove(address);
        visited.Add(address);
        result.Add(address);

        return true;
    }

    private void MarkCircularReferences(HashSet<string> circularCells)
    {
        foreach (var address in circularCells)
        {
            if (Cells.TryGetValue(address, out var cell))
            {
                cell.Value = new CellValue("#REF!");
            }
        }
    }

    private void ClearCellDependencies(Cell cell, string address)
    {
        foreach (var oldDep in cell.Dependencies)
        {
            if (_dependents.TryGetValue(oldDep, out var dependents))
            {
                dependents.Remove(address);
            }
        }

        _dependents.TryRemove(address, out _);
    }

    private void RebuildDependents()
    {
        _dependents.Clear();
        foreach (var cell in Cells.Values)
        {
            foreach (var dep in cell.Dependencies)
            {
                var dependentsList = _dependents.GetOrAdd(dep, _ => new HashSet<string>());
                dependentsList.Add(cell.Address);
            }
        }
    }
}
