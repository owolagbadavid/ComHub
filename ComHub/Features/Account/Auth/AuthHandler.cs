namespace ComHub.Features.Account.Auth;

using Api.Db;
using ComHub.Infrastructure.Database.Entities;
using ComHub.Shared.Events;
using ComHub.Shared.Exceptions;
using ComHub.Shared.Services.Utils;
using MassTransit;
using Microsoft.EntityFrameworkCore;

public class AuthHandler(
    IPublishEndpoint publishEndpoint,
    AppDbContext dbContext,
    ISecurityService securityService
)
{
    private readonly IPublishEndpoint _publishEndpoint = publishEndpoint;

    public async Task RegisterUserAsync(RegisterRequest request)
    {
        var user = new User
        {
            Email = request.Email.Trim().ToUpperInvariant(),
            PasswordHash = new Guid().ToString(),
        };
        var profile = new Profile
        {
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName?.Trim(),
            User = user,
        };

        dbContext.Users.Add(user);
        dbContext.Profiles.Add(profile);

        try
        {
            dbContext.SaveChanges();
        }
        catch (DbUpdateException ex)
        {
            HelperService.HandleDBException(ex);

            throw;
        }

        // Publish the event
        await _publishEndpoint.Publish<IUserRegisteredEvent>(
            new
            {
                UserId = user.Id,
                user.Email,
                Name = $"{profile.FirstName}",
                RegisteredAt = DateTime.UtcNow,
            }
        );
    }

    public async Task<object> LoginUserAsync(LoginRequest request)
    {
        var user =
            await dbContext.Users.FirstOrDefaultAsync(u =>
                u.Email == request.Email.Trim().ToUpperInvariant()
            ) ?? throw new UnauthorizedException("Invalid credentials");

        if (!PasswordHasher.VerifyPassword(request.Password, user.PasswordHash))
        {
            throw new UnauthorizedException("Invalid credentials");
        }

        // Generate JWT token
        var Token = securityService.SignJwt(user.Email, user.Role.ToString(), user.Id);

        return new { Token };
    }

    public async Task ResetPasswordAsync(string email, ResetPasswordRequest request)
    {
        var normalizedEmail = email.Trim().ToUpperInvariant();
        var user =
            await dbContext.Users.FirstOrDefaultAsync(u => u.Email == normalizedEmail)
            ?? throw new NotFoundException("User not found");

        // compare otp
        var otp = await securityService.RetrieveOtpAsync(normalizedEmail);

        if (otp != request.ResetCode)
            throw new UnauthorizedException("Invalid Reset Code");

        user.Password = request.Password;
        await securityService.DeleteOtpAsync(normalizedEmail);

        dbContext.Users.Update(user);
        await dbContext.SaveChangesAsync();
    }

    public async Task ForgotPasswordAsync(string email)
    {
        var user = await dbContext
            .Users.Include(u => u.Profile)
            .FirstOrDefaultAsync(u => u.Email == email);

        if (user == null)
            return;

        var profile = user.Profile;

        await _publishEndpoint.Publish<IUserForgotPasswordEvent>(
            new
            {
                UserId = user.Id,
                user.Email,
                Name = $"{profile?.FirstName ?? "User"}",
            }
        );
    }
}
