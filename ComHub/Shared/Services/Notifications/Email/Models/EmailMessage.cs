namespace ComHub.Shared.Services.Notifications.Email.Models;

using MimeKit;

public class EmailMessage
{
    public EmailMessage(
        List<EmailAddress> to,
        string subject,
        string content,
        List<EmailAddress>? cc = null
    )
    {
        To = [.. to.Select(x => new MailboxAddress(x.Name, x.Address))];
        Subject = subject;
        Content = content;

        if (cc is not null)
        {
            Cc = [.. cc.Select(x => new MailboxAddress(x.Name, x.Address))];
        }
    }

    public List<MailboxAddress> To { get; set; }
    public string Subject { get; set; }
    public string Content { get; set; }
    public List<MailboxAddress>? Cc { get; set; }
}
