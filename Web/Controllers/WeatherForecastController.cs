using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using UserManagement.DTOs;
using UserManagement.Interfaces;
using Microsoft.AspNetCore.Authorization;
namespace Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class test :ControllerBase
    {
        [HttpGet("TestExceptionHandling")]
        public IActionResult Test()
        {
            int num = 0;
            int ans = 10 / num; 

            return Ok(10);
        }
        [HttpGet]
        [Authorize]
        public IActionResult TestAuth()
        {
            return Ok("Auth is Working");

        }
    }

}
