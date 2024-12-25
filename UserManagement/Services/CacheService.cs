using ErrorOr;
using Microsoft.Extensions.Caching.Memory;
using UserManagement.Entites;
using UserManagement.Interfaces;

namespace UserManagement.Services
{
    public class CacheService  : ICacheService 
    {
        private readonly IMemoryCache _cache;
        private readonly List<string> _cacheKeys;
        private const string CacheKeysList = "CacheKeys";
        public CacheService(IMemoryCache cache)
        {
            _cache = cache;
            _cacheKeys = new List<string>();
        }

        public void AddToCache(CacheItem item, int durationInSeconds)
        {
            _cache.Set(item.Key, item.Value, TimeSpan.FromSeconds(durationInSeconds));

            TrackCacheKey(item.Key);
        }

        public List<CacheItem > GetCacheContents()
        {
            if (!_cache.TryGetValue(CacheKeysList, out List<string> keys))
            {
                keys = new List<string>();
            }

            var result = new List<CacheItem>(); 
            foreach (var key in keys)
            {
                if (_cache.TryGetValue(key, out var value))
                {
                    result.Add(new CacheItem { Key = key, Value = value ?? "" });
                }
            }
            return result;
        }
        public ErrorOr<object> GetCacheItemByKey(string key)
        {
            if (_cache.TryGetValue(key, out var value))
            {
                return value ?? Error.NotFound(description: " ");
            }
            return Error.NotFound("Key Not Found")
        }
        private void TrackCacheKey(string key)
        {
            if (!_cache.TryGetValue(CacheKeysList, out List<string> keys))
            {
                keys = new List<string>();
            }

            if (!keys.Contains(key))
            {
                keys.Add(key);
                _cache.Set(CacheKeysList, keys);
            }
        }
    }
}
