using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using ErrorOr;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using UserManagement.Interfaces;

namespace UserManagement.Services
{
    public class CloudinaryService : ICloudinaryService
    {
        private readonly Cloudinary _cloudinary;
        private readonly HttpClient _httpClient;
        private readonly ILogger<CloudinaryService> _logger;
        public CloudinaryService(Cloudinary cloudinary, HttpClient httpClient, ILogger<CloudinaryService> logger)
        {
            _cloudinary = cloudinary ?? throw new ArgumentNullException(nameof(cloudinary));
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _logger = logger;
        }

        public async Task<ErrorOr<Uri>> UploadImageAsync(IFormFile? imageFile)
        {
            try
            {

                if (imageFile == null)
                    return ErrorOr.Error.Validation("InvalidImage", "Image file cannot be null.");

                using var stream = imageFile.OpenReadStream();
                var uploadParams = new ImageUploadParams
                {
                    File = new FileDescription(imageFile.FileName, stream),
                    Folder = "Users Pics"
                };

                var uploadResult = await _cloudinary.UploadAsync(uploadParams);

                if (uploadResult.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    return uploadResult.SecureUrl;
                }

                return ErrorOr.Error.Failure("UploadFailed", "Image upload failed.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred during image upload");
                return ErrorOr.Error.Failure("UploadException", "An unexpected error occurred while uploading the image.");
            }
        }
        public async Task<ErrorOr<byte[]>> DownloadImageAsync(string imageUrl)
        {

            if (string.IsNullOrEmpty(imageUrl))
                return ErrorOr.Error.Validation("InvalidUrl", "Image URL cannot be null or empty.");

            try
            {
                var imageData = await _httpClient.GetByteArrayAsync(imageUrl);
                return imageData;
            }
            catch (Exception)
            {
                return ErrorOr.Error.Failure("DownloadFailed", "Failed to download image.");
            }
        }
    }
}
