namespace UserManagement.DTOs
{
    public class ChangePasswordRequest
    {
        public string userIdentifier { get; set; }
        public string CurrentPassword { get; set; }
        public string NewPassword { get; set; }
    }

}
