using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using ExcelClone.Domain.Cells;
using ExcelClone.Domain.ExpressionLogic;

namespace ExcelClone.Domain.Tables;

public class Table
{
    public string Id { get; private init; }

    private readonly ConcurrentDictionary<Address, Cell> _cells = new();
    private readonly Parser _parser = new();
    
    private readonly ConcurrentDictionary<Address, HashSet<Address>> _dependents = new();

    public Table(string id)
    {
        Id = id;
    }

    public Cell GetCell(Address address)
    {
        return _cells.GetOrAdd(address, address => new Cell(address));
    }

    public void SetExpression(Address address, string expression)
    {
        ArgumentNullException.ThrowIfNull(expression);
        ArgumentNullException.ThrowIfNull(address);

        var cell = GetCell(address);
        
        var oldDependencies = new HashSet<Address>(cell.Dependencies);
        
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
                dependents = new HashSet<Address>();
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
    
    private List<Address> GetCalculationOrder()
    {
        var result = new List<Address>();
        var visited = new HashSet<Address>();
        var temporaryMark = new HashSet<Address>();
        
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
    
    private bool Visit(Address address, HashSet<Address> visited, HashSet<Address> temporaryMark, List<Address> result)
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
            foreach (var dependency in cell.Dependencies)
            {
                if (!Visit(dependency, visited, temporaryMark, result))
                {
                    return false;
                }
            }
        }
        
        temporaryMark.Remove(address);
        visited.Add(address);
        result.Add(address);
        
        return true;
    }
    
    private void MarkCircularReferences(HashSet<Address> circularCells)
    {
        foreach (var address in circularCells)
        {
            if (_cells.TryGetValue(address, out var cell))
            {
                cell.Value = new CellValue("#REF!");
            }
        }
    }
}