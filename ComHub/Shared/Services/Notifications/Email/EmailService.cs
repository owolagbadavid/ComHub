namespace ComHub.Shared.Services.Notifications.Email;

using ComHub.Shared.Config;
using ComHub.Shared.Services.Notifications.Email.Models;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Options;
using MimeKit;

public class EmailService(IOptions<Config> config) : IEmailService
{
    private readonly MailConfig _config = config.Value.Mail;

    public async Task SendEmail(MimeMessage message)
    {
        using var client = new SmtpClient();
        client.Connect(_config.Host, 587, false);

        // Note: only needed if the SMTP server requires authentication
        client.Authenticate(_config.Username, _config.Password);

        var res = await client.SendAsync(message);
        Console.WriteLine("res: " + res);
        client.Disconnect(true);
    }

    // private static async Task<string> SendAsync(SmtpClient client, MimeMessage message)
    // {
    //     return "done";
    // }

    public MimeMessage CreateMessage(
        List<EmailAddress> to,
        string subject,
        string content,
        List<EmailAddress>? cc
    )
    {
        Console.WriteLine($"EmailService created  {_config.FromEmail}");

        var emailMessage = new EmailMessage(to, subject, content, cc);
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_config.FromName, _config.FromEmail));
        message.To.AddRange(emailMessage.To);
        message.Subject = emailMessage.Subject;
        message.Body = new TextPart("plain") { Text = emailMessage.Content };

        if (emailMessage.Cc is not null)
        {
            message.Cc.AddRange(emailMessage.Cc);
        }

        return message;
    }
}
