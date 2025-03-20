using Microsoft.AspNetCore.Http;

namespace UserManagement.DTOs
{
    public class RegisterModel
    {
        public string? FirstName { get; set; }

        public string? LastName { get; set; }

        public string UserName { get; set; }
        public string Email { get; set; }
        public IFormFile? Image { get; set; }

        public string Password { get; set; }
    }
}
