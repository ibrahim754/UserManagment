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
            try {
                await _mailService.SendEmailAsync(mailRequest.ToEmail, mailRequest.Subject, mailRequest.Body, mailRequest.attachments);
                return Ok("send suffully");
            }catch (Exception ex)
            {
                return 
            }
            }
    }
}
