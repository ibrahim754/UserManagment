using ErrorOr;

namespace UserManagement.Errors
{
    public static class MailErrors
    {
        public static Error InvalidEmail => Error.Validation(
            code: "InvalidEmail",
            description: "The provided email address is invalid."
        );

        public static Error FailedToSendEmail => Error.Failure(
            code: "FailedToSendEmail",
            description: "An error occurred while sending the email. Please try again later."
        );

        public static Error AttachmentError => Error.Failure(
            code: "AttachmentError",
            description: "An error occurred while handling email attachments."
        );

        public static Error SmtpConnectionFailed => Error.Failure(
            code: "SmtpConnectionFailed",
            description: "Failed to connect to the SMTP server. Please check your SMTP configuration."
        );

        public static Error AuthenticationFailed => Error.Failure(
            code: "AuthenticationFailed",
            description: "Failed to authenticate with the SMTP server. Please check your credentials."
        );
    }
}
