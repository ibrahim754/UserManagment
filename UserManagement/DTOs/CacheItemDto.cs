using UserManagement.Entites;

namespace UserManagement.DTOs
{
    public class CacheItemDto : CacheItem
    {
        public int durationInSeconds
        {
            get; set;
        }
    }
}
