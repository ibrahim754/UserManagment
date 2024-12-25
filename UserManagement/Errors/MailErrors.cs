using ErrorOr;

namespace UserManagement.Errors
{
    public class MailErrors
    {
        public static Error InvalidEmail => Error.Validation("InvalidEmail", "Email address is invalid.");
        public static Error AttachmentError => Error.Failure("AttachmentError", "Error occurred while attaching the file.");
        public static Error EmailSendFailed => Error.Failure("EmailSendFailed", "Failed to send the email.");
        public static Error SmtpConnectionFailed => Error.Failure("SmtpConnectionFailed", "Failed to connect to the SMTP server.");
        public static Error AuthenticationFailed => Error.Failure("AuthenticationFailed", "Failed to authenticate with the SMTP server.");
    }
}
