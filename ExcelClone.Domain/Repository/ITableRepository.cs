using ExcelClone.Domain.Tables;

namespace ExcelClone.Domain.Repository;

public interface ITableRepository
{
    Task<Table?> LoadAsync(string id);
    Task UpdateLiveAsync(Table table);
    Task FlushToPersistentAsync(string id);
}