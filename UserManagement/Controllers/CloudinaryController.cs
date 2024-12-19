using Microsoft.AspNetCore.Mvc;
using UserManagement.Interfaces;
using ErrorOr;
using Microsoft.AspNetCore.Http;
using Microsoft.Build.Framework;
using Microsoft.Extensions.Logging;

namespace UserManagement.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CloudinaryController : ControllerBase
    {
        private readonly ICloudinaryService _cloudinaryService;
        private readonly ILogger<CloudinaryController> _logger;
        public CloudinaryController(ICloudinaryService cloudinaryService, ILogger<CloudinaryController> logger)
        {
            _cloudinaryService = cloudinaryService;
            _logger = logger;
        }

        // Endpoint to upload an image
        [HttpPost("upload")]
        public async Task<IActionResult> UploadImage([FromForm] IFormFile imageFile)
        {
            try
            {
                ErrorOr<Uri> result = await _cloudinaryService.UploadImageAsync(imageFile);

                return result.Match<IActionResult>(
                    success => Ok(new { Url = success.ToString() }),
                    error => Problem(statusCode: 400, detail: error.First().Description)
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An Error Occoured While Uploading the image");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred during Uploading the image.");

            }
        }

        // Endpoint to download an image using URL
        [HttpGet("download")]
        public async Task<IActionResult> DownloadImage([FromQuery] string imageUrl)
        {
            try
            {
                ErrorOr<byte[]> result = await _cloudinaryService.DownloadImageAsync(imageUrl);

                return result.Match<IActionResult>(
                    success => File(success, "image/jpeg"),
                    error => Problem(statusCode: 400, detail: error.First().Description)
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An Error Occoured While Downloading the image");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred during Downloading the image.");
            }
        }
    }
}
