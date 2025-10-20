using ExcelClone.Domain.Tables;

namespace ExcelClone.Infrastructure;

public class GoogleDriveRepository
{
    public Task<Table?> LoadAsync(string id)
    {
        throw new NotImplementedException("Google Drive integration is not yet implemented.");
    }

    public Task SaveAsync(Table table)
    {
        throw new NotImplementedException("Google Drive integration is not yet implemented.");
    }
}