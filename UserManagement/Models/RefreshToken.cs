﻿
using System.Text.Json.Serialization;

namespace UserManagement.Models
{
    public class RefreshToken
    {
        public Guid Id { get; set; }
        public string Token { get; set; }
        public DateTime ExpiresOn { get; set; }
        public bool IsExpired => DateTime.UtcNow >= ExpiresOn;
        public DateTime CreatedOn { get; set; }
        public DateTime? RevokedOn { get; set; }
        public bool IsActive => RevokedOn == null && !IsExpired;
        public string? UserIp { get; set; }
        public string? UserDevice { get; set; }
        public string UserId { get; set; }
        [JsonIgnore]
        public User User {  get; set; }
      
    }
}
