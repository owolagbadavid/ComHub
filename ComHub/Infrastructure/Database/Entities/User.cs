namespace ComHub.Infrastructure.Database.Entities;

using ComHub.Infrastructure.Database.Entities.Enums;
using ComHub.Shared.Services.Utils;

public class User : BaseEntity
{
    public required string Email { get; set; }

    public required string PasswordHash { get; set; }

    // password setter to set password hash
    public string Password
    {
        set => PasswordHash = PasswordHasher.HashPassword(value);
    }
    public Profile? Profile { get; set; }
    public UserRole Role { get; set; } = UserRole.User;

    public UserStatus Status { get; set; } = UserStatus.Pending;
}
