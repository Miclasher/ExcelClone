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

    public async Task AddRowAsync(string tableId)
    {
        var table = await GetOrCreateTable(tableId);
        table.AddRow();
        await _repository.UpdateLiveAsync(table);
    }

    public async Task RemoveLastRowAsync(string tableId)
    {
        var table = await GetOrCreateTable(tableId);
        table.RemoveLastRow();
        await _repository.UpdateLiveAsync(table);
    }

    public async Task AddColumnAsync(string tableId)
    {
        var table = await GetOrCreateTable(tableId);
        table.AddColumn();
        await _repository.UpdateLiveAsync(table);
    }

    public async Task RemoveLastColumnAsync(string tableId)
    {
        var table = await GetOrCreateTable(tableId);
        table.RemoveLastColumn();
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

    private static TableDto MapToDto(Table table)
    {
        var allCells = table.GetAllCells();
        if (!allCells.Any())
        {
            return CreateEmptyGridDto(table.Id, 20, 10);
        }

        var cellMap = allCells.ToDictionary(c => c.Address, c => c);

        var (maxCol, maxRow) = table.GetDimensions();

        var grid = new List<List<CellDto>>();
        for (var r = 0; r <= maxRow; r++)
        {
            var rowList = new List<CellDto>();
            for (var c = 0; c <= maxCol; c++)
            {
                var address = AddressFormater.FormatAddress(c, r);
                rowList.Add(cellMap.TryGetValue(address, out var cell)
                    ? new CellDto(cell.Address, cell.RawExpression, cell.Value.ToString())
                    : new CellDto(address, "", ""));
            }
            grid.Add(rowList);
        }

        return new TableDto(table.Id, grid);
    }

    private static TableDto CreateEmptyGridDto(string id, int rows, int cols)
    {
        var grid = new List<List<CellDto>>();
        for (var r = 0; r < rows; r++)
        {
            var rowList = new List<CellDto>();
            for (var c = 0; c < cols; c++)
            {
                rowList.Add(new CellDto(AddressFormater.FormatAddress(c, r), "", ""));
            }
            grid.Add(rowList);
        }
        return new TableDto(id, grid);
    }
}