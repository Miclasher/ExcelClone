using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using ExcelClone.Domain.Cells;
using ExcelClone.Domain.ExpressionLogic;

namespace ExcelClone.Domain.Tables;

public class Table
{
    public string Id { get; private init; }

    private readonly ConcurrentDictionary<Address, Cell> _cells;
    private readonly Parser _parser = new();

    public Table(string id)
    {
        Id = id;
    }

    public Cell GetCell(Address address)
    {
        return _cells.GetOrAdd(address, address => new Cell(address));
    }
}