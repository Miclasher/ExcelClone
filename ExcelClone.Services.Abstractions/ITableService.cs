using ExcelClone.Shared;

namespace ExcelClone.Services.Abstractions;

public interface ITableService
{
    Task<TableDto?> GetTableDtoAsync(string tableId);
    Task UpdateCellAsync(string tableId, EditCellRequest request);
    Task SaveTableToDriveAsync(string tableId);

    Task AddRowAsync(string tableId, int rowIndex);
    Task RemoveRowAsync(string tableId, int rowIndex);
    Task AddColumnAsync(string tableId, int colIndex);
    Task RemoveColumnAsync(string tableId, int colIndex);
}