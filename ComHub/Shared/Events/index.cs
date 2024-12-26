namespace ComHub.Shared.Events;

public interface IUserRegisteredEvent
{
    Guid UserId { get; }
    string Email { get; }
    string Name { get; }
    DateTime RegisteredAt { get; }
}

public interface IUserForgotPasswordEvent
{
    Guid UserId { get; }
    string Email { get; }
    string Name { get; }
}
