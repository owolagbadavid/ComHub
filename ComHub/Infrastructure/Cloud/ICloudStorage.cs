namespace ComHub.Infrastructure.Cloud;

public interface ICloudStorage
{
    Task<string> SaveFileAsync(
        IFormFile file,
        string fileName,
        string? bucketName = default,
        string? folder = default,
        CancellationToken ct = default
    );

    Task<string> GeneratePresignedUrlAsync(
        string fileName,
        string? bucketName = default,
        string? folder = default,
        CancellationToken ct = default
    );
    Task DeleteFileAsync(string url, CancellationToken ct = default);
}
