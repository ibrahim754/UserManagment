using Microsoft.AspNetCore.Mvc;
using UserManagement.Entites;
using UserManagement.Interfaces;
using UserManagement.Errors;
using UserManagement.DTOs;  // Added CacheErrors

namespace UserManagement.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CacheController : ControllerBase
    {
        private readonly ICacheService _cacheService;

        public CacheController(ICacheService cacheService)
        {
            _cacheService = cacheService;
        }

        [HttpPost("add")]
        public IActionResult AddToCache(CacheItemDto item)
        {
            try
            {
                if (item == null)
                    return BadRequest(CacheErrors.InvalidCacheItem);  // Using CacheErrors class

                _cacheService.AddToCache(new CacheItem { Key = item.Key, Value = item.Value}, item.durationInSeconds);
                return Ok($"Key '{item.Key}' added to cache with value '{item.Value}' for {item.durationInSeconds} seconds.");
            }
            catch (ArgumentNullException)
            {
                return BadRequest(CacheErrors.InvalidCacheItem);  // Using CacheErrors class
            }
            catch (Exception)
            {
                return StatusCode(500, CacheErrors.InternalServerError);  // Using CacheErrors class
            }
        }

        [HttpGet("print")]
        public ActionResult<List<CacheItem>> GetCacheContents()
        {
            try
            {
                var cacheContents = _cacheService.GetCacheContents();
                return Ok(cacheContents);
            }
            catch (Exception)
            {
                return StatusCode(500, CacheErrors.InternalServerError);  // Using CacheErrors class
            }
        }

        [HttpGet("get/{key}")]
        public ActionResult<object> GetCacheItemByKey(string key)
        {
            var result = _cacheService.GetCacheItemByKey(key);
            if (result.IsError)
            {
                return NotFound(result.Errors.FirstOrDefault().Description);  // If error occurs
            }

            return Ok(result.Value);
        }
    }
}
