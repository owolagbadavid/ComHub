using ComHub.Infrastructure.Database.Entities.Enums;

namespace ComHub.Infrastructure.Database.Entities;

public class UserMessage : BaseEntity
{
    public required Message Message { get; set; }
    public required User User { get; set; }
    public int UserId { get; set; }
    public int MessageId { get; set; }
    public MessageStatus Status { get; set; } = MessageStatus.Sent;
}
