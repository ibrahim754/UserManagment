using ErrorOr;
namespace UserManagement.Errors
{
    public static class CacheErrors
    {
        public static Error InvalidCacheItem => Error.Validation(
            "InvalidCacheItem", "Cache item cannot be null.");

        public static Error CacheKeyNotFound => Error.NotFound(
            "CacheKeyNotFound", "Cache key not found.");

        public static Error InternalServerError => Error.Failure(
            "InternalServerError", "An unexpected error occurred while processing your request.");
    }
}
