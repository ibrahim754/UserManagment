using Microsoft.AspNetCore.Http;

namespace UserManagement.DTOs
{
    public record MailRequestDto
    {
        public string ToEmail { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        public IList<IFormFile>? attachments { get; set; }
    } 
}
