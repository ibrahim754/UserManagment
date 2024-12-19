using Microsoft.AspNetCore.Identity;
using System.Text.Json.Serialization;

namespace UserManagement.Models
{
    public delegate void UserLoginSuccessHandler(IdentityResult result);
    public delegate void UserBlockHandler(IdentityResult result);   
    public class User : IdentityUser
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public Uri? Image {  get; set; }
        [JsonIgnore]
        public bool IsBlocked { get; private set; }
        public int numberOfTriesLogIn { get; private set; }
        public List<RefreshToken>? RefreshTokens { get; set; } = new List<RefreshToken>();


        public event UserLoginSuccessHandler OnLoginSuccess;
        public event UserBlockHandler OnBlockHandler;
        public User()
        {
            IsBlocked = false;
        }

        protected void SettBlockFlag(bool status = true) => IsBlocked = status;

        protected void LogInFailed() => numberOfTriesLogIn++;
        protected void LogInSuccess() => numberOfTriesLogIn = 0;
    }
}
