using Microsoft.AspNetCore.Identity;
using System.Text.Json.Serialization;

namespace UserManagement.Models
{
    public class User : IdentityUser
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public Uri? Image {  get; set; }
        public List<RefreshToken>? RefreshTokens { get; set; } = new List<RefreshToken>();
    }
}
