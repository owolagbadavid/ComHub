namespace ComHub.Infrastructure.Database.Entities
{
    public class UserConversation : BaseEntity
    {
        public required User User { get; set; }
        public required Conversation Conversation { get; set; }
        public int UserId { get; set; }
        public int ConversationId { get; set; }
    }
}
