using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
namespace Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class Test :ControllerBase
    {
        [HttpGet("TestExceptionHandling")]
        public IActionResult lest()
        {
            int num = 0;
            int ans = 10 / num; 

            return Ok(10);
        }
        [HttpGet("Auth")]
        [Authorize]
        public IActionResult TestAuth()
        {
            return Ok("Auth is Working");

        }
    }

}
