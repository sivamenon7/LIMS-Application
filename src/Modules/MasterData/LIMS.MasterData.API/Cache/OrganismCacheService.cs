using LIMS.MasterData.API.Entities;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace LIMS.MasterData.API.Cache;

public interface IOrganismCacheService
{
    Task<Organism?> GetAsync(Guid id);
    Task SetAsync(Guid id, Organism organism, TimeSpan? expiration = null);
    Task RemoveAsync(Guid id);
}

public class OrganismCacheService : IOrganismCacheService
{
    private readonly IDistributedCache _cache;
    private const string KeyPrefix = "organism:";
    private static readonly TimeSpan DefaultExpiration = TimeSpan.FromMinutes(30);

    public OrganismCacheService(IDistributedCache cache)
    {
        _cache = cache;
    }

    public async Task<Organism?> GetAsync(Guid id)
    {
        var key = GetKey(id);
        var cached = await _cache.GetStringAsync(key);

        if (string.IsNullOrEmpty(cached))
            return null;

        return JsonSerializer.Deserialize<Organism>(cached);
    }

    public async Task SetAsync(Guid id, Organism organism, TimeSpan? expiration = null)
    {
        var key = GetKey(id);
        var serialized = JsonSerializer.Serialize(organism);
        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = expiration ?? DefaultExpiration
        };

        await _cache.SetStringAsync(key, serialized, options);
    }

    public async Task RemoveAsync(Guid id)
    {
        var key = GetKey(id);
        await _cache.RemoveAsync(key);
    }

    private static string GetKey(Guid id) => $"{KeyPrefix}{id}";
}
