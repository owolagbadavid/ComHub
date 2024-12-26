namespace ComHub.Shared.Services.Utils;

public interface ISecurityService
{
    Task<string> GenOtpAsync(string email, TimeSpan? expiry);
    Task<string?> RetrieveOtpAsync(string email);
    Task DeleteOtpAsync(string email);
    string SignJwt(string email, string role, int userId);
}
