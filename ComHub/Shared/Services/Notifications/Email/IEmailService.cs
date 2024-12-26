using ComHub.Shared.Services.Notifications.Email.Models;
using MailKit.Net.Smtp;
using MimeKit;

namespace ComHub.Shared.Services.Notifications.Email;

public interface IEmailService
{
    Task SendEmail(MimeMessage message);
    MimeMessage CreateMessage(
        List<EmailAddress> to,
        string subject,
        string content,
        List<EmailAddress>? cc
    );
}
