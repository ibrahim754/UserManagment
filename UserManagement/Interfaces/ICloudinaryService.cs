using ErrorOr;
using Microsoft.AspNetCore.Http;

namespace UserManagement.Interfaces
{
    public interface ICloudinaryService
    {
        Task<ErrorOr<Uri>> UploadImageAsync(IFormFile imageFile);
        Task<ErrorOr<byte[]>> DownloadImageAsync(string imageUrl);
        
    }

}
