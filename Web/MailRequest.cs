namespace Web
{
    public record MailRequest
    {
        public string ToEmail {  get; set; }
        public string Subject {  get; set; }
        public string Body { get; set; }    
        public IList<IFormFile>? attachments { get; set; }
    }
}
