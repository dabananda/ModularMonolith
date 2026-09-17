using Microsoft.Extensions.Caching.Distributed;
using ModularMonolith.Shared.Interfaces;
using System.Text.Json;

namespace ModularMonolith.Shared.Services
{
    public sealed class DistributedCacheService(IDistributedCache _cache) : ICacheService
    {
        public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
        {
            var cachedBytes = await _cache.GetAsync(key, cancellationToken);

            if (cachedBytes is null || cachedBytes.Length == 0)
            {
                return default;
            }

            return JsonSerializer.Deserialize<T>(cachedBytes, JsonOptions);
        }

        public async Task SetAsync<T>(string key, T value, TimeSpan expiration, CancellationToken cancellationToken = default)
        {
            var serializedBytes = JsonSerializer.SerializeToUtf8Bytes(value, JsonOptions);

            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = expiration
            };

            await _cache.SetAsync(key, serializedBytes, options, cancellationToken);
        }

        public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
        {
            await _cache.RemoveAsync(key, cancellationToken);
        }

        public async Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default)
        {
            var value = await _cache.GetAsync(key, cancellationToken);

            return value is not null;
        }

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };
    }
}