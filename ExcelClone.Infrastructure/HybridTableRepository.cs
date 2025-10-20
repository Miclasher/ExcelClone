using ExcelClone.Domain.Repository;
using ExcelClone.Domain.Tables;
using Microsoft.Extensions.Caching.Memory;

namespace ExcelClone.Infrastructure;

public class HybridTableRepository : ITableRepository
{
    private readonly IMemoryCache _memoryCache;
    private readonly GoogleDriveRepository _persistentRepository;
    private readonly MemoryCacheEntryOptions _cacheOptions = new MemoryCacheEntryOptions()
        .SetSlidingExpiration(TimeSpan.FromMinutes(30));

    public HybridTableRepository(IMemoryCache memoryCache, GoogleDriveRepository persistentRepository)
    {
        _memoryCache = memoryCache;
        _persistentRepository = persistentRepository;
    }

    public async Task<Table?> LoadAsync(string id)
    {
        var cacheKey = id.ToUpper();
        if (_memoryCache.TryGetValue(cacheKey, out Table? table))
        { 
            return table;
        }
        
        try
        {
            var persistentTable = await _persistentRepository.LoadAsync(id);
            if (persistentTable != null)
            {
                _memoryCache.Set(cacheKey, persistentTable, _cacheOptions);
            }
            return persistentTable;
        }
        catch (NotImplementedException)
        {
            Console.WriteLine($"GoogleDrive Load not implemented. Creating a new table for session.");
            var newTable = new Table(id);
            _memoryCache.Set(cacheKey, newTable, _cacheOptions);
            return newTable;
        }
    }

    public Task UpdateLiveAsync(Table table)
    {
        _memoryCache.Set(table.Id.ToUpper(), table, _cacheOptions);
        return Task.CompletedTask;
    }

    public async Task FlushToPersistentAsync(string id)
    {
        var cacheKey = id.ToUpper();
        if (_memoryCache.TryGetValue(cacheKey, out Table? tableToSave))
        {
            await _persistentRepository.SaveAsync(tableToSave!);
        }
    }
}