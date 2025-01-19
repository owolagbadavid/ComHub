namespace ComHub.Infrastructure.Database.Entities
{
    public class Message : BaseEntity
    {
        public required string Content { get; set; }
        public required User Sender { get; set; }
        public int SenderId { get; set; }
        public required Conversation Conversation { get; set; }
        public int ConversationId { get; set; }
    }
}
