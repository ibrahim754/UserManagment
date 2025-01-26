using ErrorOr;

namespace UserManagement.Errors
{
    public static class CloudinaryErrors
    {
        public static Error InvalidImage => Error.Validation(
            code: "InvalidImage",
            description: "The image provided is invalid. Please upload a valid image."
        );

        public static Error UploadFailed => Error.Failure(
            code: "UploadFailed",
            description: "The image upload failed. Please try again."
        );

        public static Error UploadException => Error.Failure(
            code: "UploadException",
            description: "An exception occurred during the image upload process."
        );

        public static Error InvalidUrl => Error.Validation(
            code: "InvalidUrl",
            description: "The provided URL is invalid."
        );

        public static Error DownloadFailed => Error.Failure(
            code: "DownloadFailed",
            description: "The image download failed. Please check the URL and try again."
        );
    }
}
