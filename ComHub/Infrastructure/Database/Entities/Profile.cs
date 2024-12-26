namespace ComHub.Infrastructure.Database.Entities;

public class Profile : BaseEntity
{
    public required string FirstName { get; set; }

    public string? LastName { get; set; }

    public string FullName => $"{FirstName}{(LastName != null ? $" {LastName}" : string.Empty)}";

    public string? ProfilePicture { get; set; }

    public string? Bio { get; set; }

    public string? Website { get; set; }

    public string? Location { get; set; }

    // Foreign key to User
    public int UserId { get; set; }

    // Navigation property to User
    public required User User { get; set; }
}
