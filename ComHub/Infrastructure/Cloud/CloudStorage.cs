using Amazon.S3;
using Amazon.S3.Model;
using ComHub.Shared.Config;
using Microsoft.Extensions.Options;

namespace ComHub.Infrastructure.Cloud;

public class CloudStorage(IAmazonS3 s3Client, Config config) : ICloudStorage
{
    private readonly CloudConfig awsOptions = config.CloudConfig;

    public async Task<string> SaveFileAsync(
        IFormFile file,
        string fileName,
        string? bucketName = default,
        string? folder = default,
        CancellationToken ct = default
    )
    {
        bucketName ??= awsOptions.BucketName;
        folder ??= awsOptions.GeneralFolder;
        var key = $"{folder}/{fileName}";

        try
        {
            using var stream = file.OpenReadStream();

            var request = new PutObjectRequest
            {
                BucketName = bucketName,
                Key = key,
                InputStream = stream,
                ContentType = file.ContentType,
                AutoCloseStream = true,
            };

            var response = await s3Client.PutObjectAsync(request, ct);
            string? fileUrl = null;
            if (response.HttpStatusCode == System.Net.HttpStatusCode.OK)
                fileUrl = $"https://{bucketName}.s3.{awsOptions.Region}.amazonaws.com/{key}";
            else
                throw new Exception("Error saving file to cloud storage");

            return fileUrl;
        }
        catch (Exception e)
        {
            throw new Exception("Error saving file to cloud storage", e);
        }
    }

    public async Task DeleteFileAsync(string url, CancellationToken ct = default)
    {
        var key = url.Split("amazonaws.com/")[1];
        var bucketName = url.Split("https://")[1].Split(".s3.")[0];
        var request = new DeleteObjectRequest { BucketName = bucketName, Key = key };

        try
        {
            var response = await s3Client.DeleteObjectAsync(request, cancellationToken: ct);
            if (response.HttpStatusCode != System.Net.HttpStatusCode.NoContent)
                throw new Exception("Error deleting file from cloud storage");
        }
        catch (Exception e)
        {
            throw new Exception("Error deleting file from cloud storage", e);
        }
    }

    public async Task<string> GeneratePresignedUrlAsync(
        string fileName,
        string? bucketName = default,
        string? folder = default,
        CancellationToken ct = default
    )
    {
        bucketName ??= awsOptions.BucketName;
        folder ??= awsOptions.GeneralFolder;
        var key = $"{folder}/{fileName}";

        try
        {
            var request = new GetPreSignedUrlRequest
            {
                BucketName = bucketName,
                Key = key,
                Expires = DateTime.Now.AddMinutes(5),
                Protocol = Protocol.HTTPS,
                Verb = HttpVerb.PUT,
            };

            var response = await s3Client.GetPreSignedURLAsync(request);
            return response;
        }
        catch (Exception e)
        {
            throw new Exception("Error generating presigned URL", e);
        }
    }
}
