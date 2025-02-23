using Microsoft.AspNetCore.Http;

namespace UserManagement.Entites
{
    public class ConfirmationUserModel
    {
        public string? FirstName { get; set; }

        public string? LastName { get; set; }

        public string Username { get; set; }
        public string Email { get; set; }
        public Uri? Image { get; set; }

        public string Password { get; set; }
    }
}
