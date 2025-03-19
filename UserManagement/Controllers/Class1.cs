using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace UserManagement.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class Class1:ControllerBase
    {
        [HttpGet]
        public IActionResult TestEndpoint()
        {
            int num  = 0;
            int ans = 10 / num;
            return Ok(ans);
        }
    }
}
