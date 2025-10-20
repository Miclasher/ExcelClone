using ExcelClone.Domain.Repository;
using ExcelClone.Domain.Tables;
using ExcelClone.Services.Abstractions;
using ExcelClone.Shared;

namespace ExcelClone.Services;

public class TableService : ITableService
{
    private readonly ITableRepository _repository;
    public TableService(ITableRepository repository) { _repository = repository; }

    public async Task<TableDto?> GetTableDtoAsync(string tableId)
    {
        var table = await GetOrCreateTable(tableId);
        return MapToDto(table);
    }

    public async Task UpdateCellAsync(string tableId, EditCellRequest request)
    {
        var table = await GetOrCreateTable(tableId);
        table.SetExpression(request.CellAddress, request.NewExpression);
        await _repository.UpdateLiveAsync(table);
    }

    public async Task AddRowAsync(string tableId, int rowIndex)
    {
        var table = await GetOrCreateTable(tableId);
        table.AddRow(rowIndex);
        await _repository.UpdateLiveAsync(table);
    }

    public async Task RemoveRowAsync(string tableId, int rowIndex)
    {
        var table = await GetOrCreateTable(tableId);
        table.RemoveRow(rowIndex);
        await _repository.UpdateLiveAsync(table);
    }

    public async Task AddColumnAsync(string tableId, int colIndex)
    {
        var table = await GetOrCreateTable(tableId);
        table.AddColumn(colIndex);
        await _repository.UpdateLiveAsync(table);
    }

    public async Task RemoveColumnAsync(string tableId, int colIndex)
    {
        var table = await GetOrCreateTable(tableId);
        table.RemoveColumn(colIndex);
        await _repository.UpdateLiveAsync(table);
    }

    public Task SaveTableToDriveAsync(string tableId)
    {
        return _repository.FlushToPersistentAsync(tableId);
    }

    private async Task<Table> GetOrCreateTable(string tableId)
    {
        return await _repository.LoadAsync(tableId) ?? new Table(tableId);
    }

    private TableDto MapToDto(Table table)
    {
        var allCells = table.GetAllCells();
        if (!allCells.Any())
        {
            return CreateEmptyGridDto(table.Id, 20, 10);
        }

        var cellMap = allCells.ToDictionary(c => c.Address, c => c);

        var maxRow = 19;
        var maxCol = 9;
        foreach (var cell in allCells)
        {
            var (col, row) = Table.ParseAddress(cell.Address);
            if (row > maxRow) maxRow = row;
            if (col > maxCol) maxCol = col;
        }

        var grid = new List<List<CellDto>>();
        for (var r = 0; r <= maxRow; r++)
        {
            var rowList = new List<CellDto>();
            for (var c = 0; c <= maxCol; c++)
            {
                string address = Table.FormatAddress(c, r);
                rowList.Add(cellMap.TryGetValue(address, out var cell)
                    ? new CellDto(cell.Address, cell.RawExpression, cell.Value.ToString())
                    : new CellDto(address, "", ""));
            }
            grid.Add(rowList);
        }

        return new TableDto(table.Id, grid);
    }

    private TableDto CreateEmptyGridDto(string id, int rows, int cols)
    {
        var table = new Table(id);
        var grid = new List<List<CellDto>>();
        for (var r = 0; r < rows; r++)
        {
            var rowList = new List<CellDto>();
            for (var c = 0; c < cols; c++)
            {
                rowList.Add(new CellDto(Table.FormatAddress(c, r), "", ""));
            }
            grid.Add(rowList);
        }
        return new TableDto(id, grid);
    }
}