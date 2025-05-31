using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Options;
using MyChat.Configuration;

namespace MyChat.Services;

public class S3ImageStorageService : IImageStorageService
{
    private readonly IAmazonS3 _s3Client;
    private readonly string _bucketName;
    private readonly string _region;
    private readonly ILogger<S3ImageStorageService> _logger;

    public S3ImageStorageService(IAmazonS3 s3Client, IOptions<AwsSettings> awsSettings, ILogger<S3ImageStorageService> logger)
    {
        _s3Client = s3Client;
        _logger = logger;
        
        // Check environment variables first (Docker Compose), then fallback to config
        var envBucketName = Environment.GetEnvironmentVariable("AWS__S3__BucketName");
        var configBucketName = awsSettings.Value.S3?.BucketName;
        
        _bucketName = envBucketName ?? configBucketName 
                     ?? throw new InvalidOperationException("S3 bucket name not configured");
                     
        var envRegion = Environment.GetEnvironmentVariable("AWS__S3__Region");
        var configRegion = awsSettings.Value.S3?.Region;
        
        _region = envRegion ?? configRegion ?? "us-east-1";
        
        // Log configuration details
        _logger.LogInformation("S3ImageStorageService initialized:");
        _logger.LogInformation("  Environment AWS__S3__BucketName: {EnvBucket}", envBucketName ?? "(not set)");
        _logger.LogInformation("  Config S3.BucketName: {ConfigBucket}", configBucketName ?? "(not set)");
        _logger.LogInformation("  Final bucket name: {BucketName}", _bucketName);
        _logger.LogInformation("  Environment AWS__S3__Region: {EnvRegion}", envRegion ?? "(not set)");
        _logger.LogInformation("  Config S3.Region: {ConfigRegion}", configRegion ?? "(not set)");
        _logger.LogInformation("  Final region: {Region}", _region);
    }

    public async Task<string> UploadImageAsync(IFormFile file, string fileName)
    {
        try
        {
            _logger.LogInformation("Attempting to upload image to S3:");
            _logger.LogInformation("  Bucket: {BucketName}", _bucketName);
            _logger.LogInformation("  Region: {Region}", _region);
            _logger.LogInformation("  File: {FileName}", fileName);
            _logger.LogInformation("  Size: {FileSize} bytes", file.Length);
            
            var request = new PutObjectRequest
            {
                BucketName = _bucketName,
                Key = fileName,
                InputStream = file.OpenReadStream(),
                ContentType = file.ContentType
            };

            await _s3Client.PutObjectAsync(request);
            
            var imageUrl = GetImageUrl(fileName);
            _logger.LogInformation("Successfully uploaded image to S3: {ImageUrl}", imageUrl);
            return imageUrl;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to upload image to S3. Bucket: {BucketName}, File: {FileName}", _bucketName, fileName);
            throw new InvalidOperationException($"Failed to upload image to S3 bucket '{_bucketName}': {ex.Message}", ex);
        }
    }

    public async Task<bool> DeleteImageAsync(string fileName)
    {
        try
        {
            var request = new DeleteObjectRequest
            {
                BucketName = _bucketName,
                Key = fileName
            };

            await _s3Client.DeleteObjectAsync(request);
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public async Task<(Stream imageStream, string contentType)> GetImageAsync(string fileName)
    {
        try
        {
            var request = new GetObjectRequest
            {
                BucketName = _bucketName,
                Key = fileName
            };

            var response = await _s3Client.GetObjectAsync(request);
            return (response.ResponseStream, response.Headers.ContentType);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve image from S3. Bucket: {BucketName}, File: {FileName}", _bucketName, fileName);
            throw new InvalidOperationException($"Failed to retrieve image from S3 bucket '{_bucketName}': {ex.Message}", ex);
        }
    }

    public string GetImageUrl(string fileName)
    {
        // Return API URL instead of direct S3 URL since bucket is private
        return $"/api/images/{fileName}";
    }
}
