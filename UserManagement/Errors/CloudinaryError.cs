 using ErrorOr;
namespace UserManagement.Errors
{
    public class UploadImageError
    {
        public static Error InvalidImage => Error.Validation("InvalidImage", "Image file cannot be null.");
        public static Error UploadFailed => Error.Failure("UploadFailed", "Image upload failed.");
        public static Error UploadException => Error.Failure("UploadException", "An unexpected error occurred while uploading the image.");
    }
}

 namespace UserManagement.Errors
{
    public class DownloadImageError
    {
        public static Error InvalidUrl => Error.Validation("InvalidUrl", "Image URL cannot be null or empty.");
        public static Error DownloadFailed => Error.Failure("DownloadFailed", "Failed to download image.");
    }
}
