namespace UserManagement.DTOs
{
    public class RefreshTokenRequest
    {
        public string? RefreshToken { get; set; }
        public string? UserDeviceId { get; set; }
        public string? UserIpAddress { get; set; }
    }
}
