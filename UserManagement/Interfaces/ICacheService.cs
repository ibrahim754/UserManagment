using ErrorOr;
using UserManagement.Entites;

namespace UserManagement.Interfaces
{
    public interface ICacheService 
    {
        void AddToCache(CacheItem item, int durationInSeconds);
        List<CacheItem > GetCacheContents();
        ErrorOr<object> GetCacheItemByKey(string key);
    }
}
