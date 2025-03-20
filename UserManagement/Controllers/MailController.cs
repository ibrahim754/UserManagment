using Microsoft.AspNetCore.Mvc;
using UserManagement.DTOs;
using UserManagement.Interfaces;

namespace UserManagement.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class MailController : ControllerBase
    {
        private readonly IMailService _mailService;
        public MailController(IMailService mailService)
        {
            _mailService = mailService;
        }
        [HttpPost("send")]
        public async Task<IActionResult> SendEmail([FromForm] MailRequestDto mailRequest)
        {
            await _mailService.SendEmailAsync(mailRequest);
            return Ok("send suffully");
        }
    }
}
