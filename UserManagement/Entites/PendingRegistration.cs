namespace UserManagement.Entites
{
    public class PendingRegistration
    {
        public string Email { get; set; }
        public string Username { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public Uri? Image {  get; set; }    
        public string Password { get; set; }
        public string ConfirmationCode { get; set; }      
    }
}
