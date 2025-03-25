using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using UserManagement.Interfaces;
using UserManagement.Errors;
using ErrorOr;

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
            if (imageFile == null)
                return CloudinaryErrors.InvalidImage;

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

            return CloudinaryErrors.UploadFailed;

        }

        public async Task<ErrorOr<byte[]>> DownloadImageAsync(string imageUrl)
        {
            if (string.IsNullOrEmpty(imageUrl))
                return CloudinaryErrors.InvalidUrl;

            var imageData = await _httpClient.GetByteArrayAsync(imageUrl);
            return imageData;

        }
    }
}
