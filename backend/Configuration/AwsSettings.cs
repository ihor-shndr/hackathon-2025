namespace MyChat.Configuration;

public class AwsSettings
{
    public S3Settings S3 { get; set; } = new();
}

public class S3Settings
{
    public string BucketName { get; set; } = string.Empty;
    public string Region { get; set; } = string.Empty;
    public string AccessKey { get; set; } = string.Empty;
    public string SecretKey { get; set; } = string.Empty;
}
