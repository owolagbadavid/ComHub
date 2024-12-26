namespace ComHub.Features.Account.Profile;

public class ProfileModel
{
    public required string FirstName { get; set; }
    public string? LastName { get; set; }
    public string? ProfilePicture { get; set; }
    public string? Bio { get; set; }
    public string? Website { get; set; }
    public string? Location { get; set; }
}
