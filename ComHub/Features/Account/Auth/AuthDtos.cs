using System.ComponentModel.DataAnnotations;
using FluentValidation;

namespace ComHub.Features.Account.Auth;

public class RegisterRequest
{
    [EmailAddress]
    [Required]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string FirstName { get; set; } = string.Empty;
    public string? LastName { get; set; }
}

public class LoginRequest
{
    [EmailAddress]
    [Required]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;
}

public class ResetPasswordRequest
{
    [Required]
    public string Password { get; set; } = string.Empty;

    [Required]
    public string ResetCode { get; set; } = string.Empty;
}

public class ForgotPasswordRequest
{
    [EmailAddress]
    [Required]
    public string Email { get; set; } = string.Empty;
}

sealed class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
    public RegisterRequestValidator()
    {
        RuleFor(x => x.Email).EmailAddress();
        RuleFor(x => x.FirstName).NotEmpty();
    }
}

sealed class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.Email).EmailAddress();
        RuleFor(x => x.Password).NotEmpty().MinimumLength(6);
    }
}

sealed class ResetPasswordRequestValidator : AbstractValidator<ResetPasswordRequest>
{
    public ResetPasswordRequestValidator()
    {
        RuleFor(x => x.Password).NotEmpty().MinimumLength(6);
        RuleFor(x => x.ResetCode).NotEmpty();
    }
}

sealed class ForgotPasswordRequestValidator : AbstractValidator<ForgotPasswordRequest>
{
    public ForgotPasswordRequestValidator()
    {
        RuleFor(x => x.Email).EmailAddress();
    }
}
