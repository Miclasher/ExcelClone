using ExcelClone.Shared;

namespace ExcelClone.Services.Abstractions;

public interface ITableService
{
    Task<TableDto?> GetTableDtoAsync(string tableId);
    Task UpdateCellAsync(string tableId, EditCellRequest request);
    Task SaveTableToDriveAsync(string tableId);

    Task AddRowAsync(string tableId);
    Task RemoveLastRowAsync(string tableId);
    Task AddColumnAsync(string tableId);
    Task RemoveLastColumnAsync(string tableId);
}