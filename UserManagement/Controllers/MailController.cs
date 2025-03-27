using Microsoft.AspNetCore.Mvc;
using UserManagement.DTOs;
using UserManagement.Interfaces;

namespace UserManagement.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class MailController(IMailService mailService) : ControllerBase
    {
        [HttpPost("send")]
        public async Task<IActionResult> SendEmail([FromForm] MailRequestDto mailRequest)
        {
            await mailService.SendEmailAsync(mailRequest);
            return Ok("send suffully");
        }
    }
}
