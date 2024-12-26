namespace ComHub.Shared.Config;

public class Config
{
    public required ConnectionStrings ConnectionStrings { get; set; }

    public required JwtConfig Jwt { get; set; }

    public required MailConfig Mail { get; set; }

    public required JwtSettings JwtSettings { get; set; }
}

public class MailConfig
{
    public required string Host { get; set; }
    public int Port { get; set; } = 587;
    public required string Username { get; set; }
    public required string Password { get; set; }
    public required string FromEmail { get; set; }
    public required string FromName { get; set; }
}

public class JwtConfig
{
    public required string Secret { get; set; }
    public required string Issuer { get; set; }
    public required string Audience { get; set; }
    public int ExpiryMinutes { get; set; } = 60;
}

public class ConnectionStrings
{
    public required string DefaultConnection { get; set; }

    public required string RedisConnection { get; set; }
}

public class JwtSettings
{
    public required string Secret { get; set; }
    public required string Issuer { get; set; }
    public required string Audience { get; set; }
    public int ExpiryMinutes { get; set; }
}
