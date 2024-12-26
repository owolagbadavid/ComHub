namespace ComHub.Shared.Services.Utils;

public interface IUserContext
{
    int UserId { get; }

    string UserRole { get; }
}
