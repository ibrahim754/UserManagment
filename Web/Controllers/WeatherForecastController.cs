using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http;

namespace Web.Controllers
{
    [ApiController]
    [Route("[controller]")]
 
    public class WeatherForecastController : ControllerBase
    {
       private readonly IMailService _mailService;
        public WeatherForecastController(IMailService mailService)
        {
            _mailService = mailService;
        }
        [HttpPost("send")]
        public async Task<IActionResult> SendEmail([FromForm] MailRequest mailRequest)
        {
            await _mailService.SendEmailAsync(mailRequest.ToEmail, mailRequest.Subject, mailRequest.Body, mailRequest.attachments);
            return Ok( "send suffully");
        }
    }
}
