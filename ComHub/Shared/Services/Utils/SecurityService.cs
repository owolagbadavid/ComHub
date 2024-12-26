namespace ComHub.Shared.Services.Utils;

using System;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using ComHub.Shared.Config;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

public static class PasswordHasher
{
    private const int SaltSize = 16; // 128-bit
    private const int KeySize = 32; // 256-bit
    private const int Iterations = 10000;

    public static string HashPassword(string password)
    {
        // Generate a salt
        byte[] salt = RandomNumberGenerator.GetBytes(SaltSize);

        // Hash the password using PBKDF2
        using var pbkdf2 = new Rfc2898DeriveBytes(
            password,
            salt,
            Iterations,
            HashAlgorithmName.SHA256
        );
        byte[] hash = pbkdf2.GetBytes(KeySize);

        // Combine salt and hash for storage
        byte[] hashBytes = new byte[SaltSize + KeySize];
        Array.Copy(salt, 0, hashBytes, 0, SaltSize);
        Array.Copy(hash, 0, hashBytes, SaltSize, KeySize);

        // Convert to Base64 for storage
        return Convert.ToBase64String(hashBytes);
    }

    public static bool VerifyPassword(string password, string hashedPassword)
    {
        // Extract hash and salt from the stored Base64 string
        byte[] hashBytes = Convert.FromBase64String(hashedPassword);
        byte[] salt = new byte[SaltSize];
        Array.Copy(hashBytes, 0, salt, 0, SaltSize);

        byte[] storedHash = new byte[KeySize];
        Array.Copy(hashBytes, SaltSize, storedHash, 0, KeySize);

        // Hash the input password with the same salt
        using var pbkdf2 = new Rfc2898DeriveBytes(
            password,
            salt,
            Iterations,
            HashAlgorithmName.SHA256
        );
        byte[] computedHash = pbkdf2.GetBytes(KeySize);

        // Compare the computed hash with the stored hash
        return CryptographicOperations.FixedTimeEquals(computedHash, storedHash);
    }
}

public class SecurityService(IDistributedCache cache, IOptions<Config> config) : ISecurityService
{
    private readonly JwtSettings _jwtSettings = config.Value.JwtSettings;

    public async Task<string> GenOtpAsync(string email, TimeSpan? expiry = null)
    {
        var otp = new Random().Next(100000, 999999).ToString();

        var key = $"otp:{email}";

        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = expiry ?? TimeSpan.FromMinutes(10),
        };

        await cache.SetStringAsync(key, otp, options);

        return otp;
    }

    public async Task<string?> RetrieveOtpAsync(string email)
    {
        var key = $"otp:{email}";

        return await cache.GetStringAsync(key);
    }

    public async Task DeleteOtpAsync(string email)
    {
        var key = $"otp:{email}";

        await cache.RemoveAsync(key);
    }

    public string SignJwt(string email, string role, int userId)
    {
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Secret));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var handler = new JsonWebTokenHandler();
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Issuer = _jwtSettings.Issuer,
            Audience = _jwtSettings.Audience,
            Expires = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpiryMinutes),
            SigningCredentials = credentials,
            Claims = new Dictionary<string, object>
            {
                { ClaimTypes.Email, email },
                { ClaimTypes.Role, role },
                { ClaimTypes.NameIdentifier, userId },
                { JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString() },
            },
        };

        return handler.CreateToken(tokenDescriptor);
    }
}
