using ExcelClone.Domain.Cells;
using ExcelClone.Domain.ExpressionLogic;
using ExcelClone.Domain.ExpressionLogic.AstNodes;
using System.Collections.Concurrent;
using static System.Text.RegularExpressions.Regex;

namespace ExcelClone.Domain.Tables;

public class Table
{
    public string Id { get; private init; }

    private readonly ConcurrentDictionary<string, Cell> _cells = new();
    private readonly Parser _parser = new();

    private readonly ConcurrentDictionary<string, HashSet<string>> _dependents = new();

    public Table(string id)
    {
        Id = id;
    }

    public void AddRow(int rowIndex) => ShiftCells(rowIndex, -1, 1, 0);
    public void RemoveRow(int rowIndex) => ShiftCells(rowIndex, -1, -1, 0, true);
    public void AddColumn(int colIndex) => ShiftCells(-1, colIndex, 0, 1);
    public void RemoveColumn(int colIndex) => ShiftCells(-1, colIndex, 0, -1, true);

    public List<Cell> GetAllCells()
    {
        return _cells.Values.ToList();
    }

    public Cell GetCell(string address)
    {
        return _cells.GetOrAdd(address, addressOfNewCell => new Cell(addressOfNewCell));
    }

    public bool TryGetCell(string address, out Cell o)
    {
        return _cells.TryGetValue(address, out o!);
    }

    public void SetExpression(string address, string expression)
    {
        ArgumentNullException.ThrowIfNull(expression);
        ArgumentNullException.ThrowIfNull(address);

        var cell = GetCell(address);

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
            if (!_dependents.TryGetValue(newDep, out var dependents))
            {
                dependents = new HashSet<string>();
                _dependents[newDep] = dependents;
            }
            dependents.Add(address);
        }

        RecalculateAll();
    }

    private void RecalculateAll()
    {
        var calculationOrder = GetCalculationOrder();

        foreach (var address in calculationOrder)
        {
            if (_cells.TryGetValue(address, out var cell))
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

        foreach (var address in _cells.Keys)
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

        if (_cells.TryGetValue(address, out var cell))
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
            if (_cells.TryGetValue(address, out var cell))
            {
                cell.Value = new CellValue("#REF!");
            }
        }
    }

    private void ShiftCells(int startRow, int startCol, int rowOffset, int colOffset, bool isDelete = false)
    {
        var updatedCellData = new Dictionary<string, string>();
        var expressionBuilder = new AstStringBuilder();

        var cellsToRemove = isDelete
            ? _cells.Values.Where(c =>
            {
                var (col, row) = ParseAddress(c.Address);
                return (rowOffset != 0 && row == startRow) || (colOffset != 0 && col == startCol);
            }).ToHashSet()
            : [];

        foreach (var oldCell in _cells.Values)
        {
            if (cellsToRemove.Contains(oldCell)) continue;

            var (oldCol, oldRow) = ParseAddress(oldCell.Address);
            var newRow = (rowOffset != 0 && oldRow >= startRow) ? oldRow + rowOffset : oldRow;
            var newCol = (colOffset != 0 && oldCol >= startCol) ? oldCol + colOffset : oldCol;

            if (newRow < 0 || newCol < 0) continue;

            var newAddress = FormatAddress(newCol, newRow);

            var originalAst = _parser.Parse(oldCell.RawExpression);
            var updatedAst = UpdateAstReferences(originalAst, startRow, startCol, rowOffset, colOffset);
            string newExpression = expressionBuilder.Build(updatedAst);

            updatedCellData[newAddress] = newExpression;
        }

        _cells.Clear();
        _dependents.Clear();
        foreach (var (address, expression) in updatedCellData)
        {
            SetExpression(address, expression);
        }
    }

    private AstNode UpdateAstReferences(AstNode node, int startRow, int startCol, int rowOffset, int colOffset)
    {
        return node switch
        {
            BinaryOperationNode bon => new BinaryOperationNode(
                UpdateAstReferences(bon.Right, startRow, startCol, rowOffset, colOffset),
                UpdateAstReferences(bon.Left, startRow, startCol, rowOffset, colOffset),
                bon.Op
                ),

            FunctionCallNode fcn => new FunctionCallNode(fcn.FunctionName,
                fcn.Arguments.Select(arg => UpdateAstReferences(arg, startRow, startCol, rowOffset, colOffset)).ToList()),

            CellReferenceNode crn => new CellReferenceNode(GetShiftedAddress(crn.CellAddress, startRow, startCol, rowOffset, colOffset)),

            _ => node
        };
    }

    private string GetShiftedAddress(string address, int startRow, int startCol, int rowOffset, int colOffset)
    {
        var (col, row) = ParseAddress(address);
        var newRow = (rowOffset != 0 && row >= startRow) ? row + rowOffset : row;
        var newCol = (colOffset != 0 && col >= startCol) ? col + colOffset : col;
        if (newRow < 0 || newCol < 0) return "#REF!";
        return FormatAddress(newCol, newRow);
    }

    public static (int col, int row) ParseAddress(string address)
    {
        var match = Match(address.ToUpper(), @"([A-Z]+)(\d+)");
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
        if (col < 0 || row < 0) return "#REF!";
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
