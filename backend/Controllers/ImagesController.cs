using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyChat.Models.DTOs.Common;
using MyChat.Services;

namespace MyChat.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ImagesController : ControllerBase
{
    private readonly IImageStorageService _imageStorageService;
    private readonly ILogger<ImagesController> _logger;

    public ImagesController(IImageStorageService imageStorageService, ILogger<ImagesController> logger)
    {
        _imageStorageService = imageStorageService;
        _logger = logger;
    }

    [HttpPost("upload")]
    public async Task<ActionResult<ApiResponse<string>>> UploadImage(IFormFile file)
    {
        try
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest(ApiResponse<string>.ErrorResponse("No file provided"));
            }

            // Basic validation
            var allowedTypes = new[] { "image/jpeg", "image/jpg", "image/png", "image/gif" };
            if (!allowedTypes.Contains(file.ContentType.ToLower()))
            {
                return BadRequest(ApiResponse<string>.ErrorResponse("Invalid file type. Only JPEG, PNG, and GIF are allowed"));
            }

            // 10MB limit
            if (file.Length > 10 * 1024 * 1024)
            {
                return BadRequest(ApiResponse<string>.ErrorResponse("File size too large. Maximum 10MB allowed"));
            }

            // Generate unique filename
            var fileExtension = Path.GetExtension(file.FileName);
            var fileName = $"{Guid.NewGuid()}{fileExtension}";

            var imageUrl = await _imageStorageService.UploadImageAsync(file, fileName);

            _logger.LogInformation("Image uploaded successfully: {FileName}", fileName);

            return Ok(ApiResponse<string>.SuccessResponse(imageUrl, "Image uploaded successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading image");
            return StatusCode(500, ApiResponse<string>.ErrorResponse("Internal server error"));
        }
    }

    [HttpGet("{fileName}")]
    [AllowAnonymous] // Allow anonymous access to view images
    public async Task<IActionResult> GetImage(string fileName)
    {
        try
        {
            if (string.IsNullOrEmpty(fileName))
            {
                return BadRequest("File name is required");
            }

            // Basic security check - ensure filename doesn't contain path traversal
            if (fileName.Contains("..") || fileName.Contains("/") || fileName.Contains("\\"))
            {
                return BadRequest("Invalid file name");
            }

            var (imageStream, contentType) = await _imageStorageService.GetImageAsync(fileName);
            
            _logger.LogInformation("Serving image: {FileName}, ContentType: {ContentType}", fileName, contentType);
            
            return File(imageStream, contentType ?? "application/octet-stream");
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Image not found: {FileName}", fileName);
            return NotFound($"Image '{fileName}' not found");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving image: {FileName}", fileName);
            return StatusCode(500, "Internal server error");
        }
    }
}
