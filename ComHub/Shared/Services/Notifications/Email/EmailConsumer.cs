using ComHub.Shared.Events;
using ComHub.Shared.Services.Notifications.Email.Models;
using ComHub.Shared.Services.Utils;
using MassTransit;

namespace ComHub.Shared.Services.Notifications.Email;

public class EmailNotificationConsumer(IEmailService emailService, ISecurityService securityService)
    : IConsumer<IUserRegisteredEvent>,
        IConsumer<IUserForgotPasswordEvent>
{
    public async Task Consume(ConsumeContext<IUserRegisteredEvent> context)
    {
        var message = context.Message;

        var otp = await securityService.GenOtpAsync(
            email: message.Email,
            expiry: TimeSpan.FromMinutes(10)
        );

        Console.WriteLine($"otp: {otp}");

        // Send email logic
        await SendEmailAsync(message.Email, message.Name, otp);

        Console.WriteLine(
            $"Event: 'Register' -> Email sent to {message.Email} for user {message.Name}"
        );
    }

    public async Task Consume(ConsumeContext<IUserForgotPasswordEvent> context)
    {
        var message = context.Message;

        var otp = await securityService.GenOtpAsync(
            email: message.Email,
            expiry: TimeSpan.FromMinutes(10)
        );

        Console.WriteLine($"otp: {otp}");

        // Send email logic
        await SendEmailAsync(message.Email, message.Name, otp);

        Console.WriteLine(
            $"Event: 'ForgotPassword' -> Email sent to {message.Email} for user {message.Name}"
        );
    }

    private Task SendEmailAsync(string email, string fullName, string otp)
    {
        // Simulate sending an email
        Console.WriteLine($"Sending email to {email} for {fullName}");
        var message = emailService.CreateMessage(
            [new EmailAddress { Name = fullName, Address = email }],
            "Welcome to ComHub",
            $"Hello {fullName}, welcome to ComHub! \nReset your password with this one time passcode.\nThis code expires in 10 minutes",
            null
        );
        // emailService.SendEmail(message);
        return Task.CompletedTask;
    }
}
