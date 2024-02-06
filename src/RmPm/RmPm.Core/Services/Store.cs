using RmPm.Core.Contracts;
using RmPm.Core.Models;

namespace RmPm.Core.Services;

public class Store
{
    private const string StoreFilename = "store-list.json";
    
    private readonly ILocalStore _store;
    private readonly IJsonService _jsonService;

    public Store(ILocalStore store, IJsonService jsonService)
    {
        _store = store;
        _jsonService = jsonService;
    }
    
    public async Task SaveAsync(EntryStore entry, CancellationToken ctk = default)
    {
        var all = (await GetAllAsync(ctk)).ToList();
        
        var existsIndex = all.FindIndex(x => x.Id == entry.Id);
        
        if (existsIndex == -1)
        {
            entry.Id = Guid.NewGuid();
            all.Add(entry);
        }
        else
        {
            all[existsIndex] = entry;
        }

        await SaveAllAsync(all, ctk);
    }

    private async Task SaveAllAsync(IEnumerable<EntryStore> items, CancellationToken ctk = default)
    {
        var json = _jsonService.Serialize(items);
        await _store.WriteAsync(StoreFilename, json, ctk);
    }

    public async Task DeleteAsync(EntryStore entry, CancellationToken ctk = default)
    {
        var all = (await GetAllAsync(ctk)).ToList();
        var index = all.FindIndex(x => x.Id == entry.Id);
        
        if (index == -1) 
            return;

        all.RemoveAt(index);
        await SaveAllAsync(all, ctk);
    }

    public async Task<EntryStore?> FindAsync(Func<EntryStore, bool> find, CancellationToken ctk = default)
    {
        return (await GetAllAsync(ctk)).FirstOrDefault(find);
    }

    public async Task<EntryStore[]> GetAllAsync(CancellationToken ctk = default)
    {
        if (!_store.Exists(StoreFilename))
            return Array.Empty<EntryStore>();

        var json = await _store.ReadAsync(StoreFilename, ctk);
        var list = _jsonService.Deserialize<EntryStore[]>(json);

        return list ?? Array.Empty<EntryStore>();
    }
}