using ErrorOr;
using Microsoft.Extensions.Caching.Memory;
using UserManagement.Entites;
using UserManagement.Interfaces;
using UserManagement.Errors;   
namespace UserManagement.Services
{
    public class CacheService : ICacheService
    {
        private readonly IMemoryCache _cache;
        private readonly List<string> _cacheKeys;
        private const string CacheKeysList = "CacheKeys";

        public CacheService(IMemoryCache cache)
        {
            _cache = cache;
            _cacheKeys = new List<string>();
        }

        public void AddToCache(CacheItem item, long durationInSeconds)
        {
            try
            {
                if (item == null)
                    throw new ArgumentNullException(nameof(item), "Cache item cannot be null.");

                _cache.Set(item.Key, item.Value, TimeSpan.FromSeconds(durationInSeconds));
                TrackCacheKey(item.Key);
            }
            catch (Exception)
            {
                throw new InvalidOperationException("An error occurred while adding the item to the cache.");
            }
        }

        public List<CacheItem> GetCacheContents()
        {
            try
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
            catch (Exception)
            {
                throw new InvalidOperationException("An error occurred while retrieving cache contents.");
            }
        }

        public ErrorOr<object> GetCacheItemByKey(string key)
        {
            try
            {
                if (_cache.TryGetValue(key, out var value))
                {
                    return value ?? Error.NotFound("Cache item found but is empty.");
                }

                return CacheErrors.CacheKeyNotFound;  // Using CacheErrors class
            }
            catch (Exception)
            {
                return CacheErrors.InternalServerError;  // Using CacheErrors class
            }
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
