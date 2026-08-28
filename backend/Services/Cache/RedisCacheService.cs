using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Services;

public class RedisCacheService : ICacheService
{
    private readonly IDistributedCache _distributedCache;
    private readonly ILogger<RedisCacheService> _logger;

    public RedisCacheService(
        IDistributedCache distributedCache,
        ILogger<RedisCacheService>? logger = null)
    {
        _distributedCache = distributedCache;
        _logger = logger ?? NullLogger<RedisCacheService>.Instance;
    }

    public async Task<T?> GetAsync<T>(string key)
    {
        try
        {
            var cachedData = await _distributedCache.GetStringAsync(key);
            if (string.IsNullOrEmpty(cachedData))
            {
                _logger.LogDebug("Cache MISS para a chave '{Key}'", key);
                return default;
            }

            _logger.LogInformation("Cache HIT para a chave '{Key}'", key);
            return JsonSerializer.Deserialize<T>(cachedData);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Falha ao ler cache da chave '{Key}'. Prosseguindo sem cache.", key);
            return default;
        }
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null)
    {
        try
        {
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = expiration ?? TimeSpan.FromMinutes(10)
            };

            var jsonData = JsonSerializer.Serialize(value);
            await _distributedCache.SetStringAsync(key, jsonData, options);
            _logger.LogInformation("Cache gravado com sucesso para a chave '{Key}' com expiração de {Expiration}min",
                key, (expiration ?? TimeSpan.FromMinutes(10)).TotalMinutes);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Falha ao gravar cache da chave '{Key}'.", key);
        }
    }

    public async Task RemoveAsync(string key)
    {
        try
        {
            await _distributedCache.RemoveAsync(key);
            _logger.LogInformation("Chave de cache '{Key}' removida com sucesso", key);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Falha ao remover cache da chave '{Key}'.", key);
        }
    }
}
