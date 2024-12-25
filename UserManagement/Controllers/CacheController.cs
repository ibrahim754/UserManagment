using Microsoft.AspNetCore.Mvc;
using UserManagement.Entites;
using UserManagement.Interfaces;

namespace UserManagement.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CacheController : ControllerBase
    {
        private readonly ICacheService  _cacheService;  

        public CacheController(ICacheService  cacheService)
        {
            _cacheService = cacheService;
        }

        [HttpPost("add")]
        public IActionResult AddToCache(CacheItem item, int durationInSeconds = 60)
        {
            _cacheService.AddToCache( item, durationInSeconds);
            return Ok($"Key '{item.Key}' added to cache with value '{item.Value}' for {durationInSeconds} seconds.");
        }

        [HttpGet("print")]
        public ActionResult<List<CacheItem>> GetCacheContents()
        {
            var cacheContents = _cacheService.GetCacheContents();
            return Ok(cacheContents);
        }
    }

}
