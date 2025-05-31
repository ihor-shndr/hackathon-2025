namespace MyChat.Services;

public interface IImageStorageService
{
    Task<string> UploadImageAsync(IFormFile file, string fileName);
    Task<bool> DeleteImageAsync(string fileName);
    string GetImageUrl(string fileName);
    Task<(Stream imageStream, string contentType)> GetImageAsync(string fileName);
}
