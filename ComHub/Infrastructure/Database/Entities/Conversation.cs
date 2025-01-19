namespace ComHub.Infrastructure.Database.Entities
{
    public class Conversation : BaseEntity
    {
        public required ICollection<User> Users { get; set; } = [];
        public List<Message> Messages { get; set; } = [];
    }
}
