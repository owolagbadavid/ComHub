using System.ComponentModel.DataAnnotations.Schema;

namespace ComHub.Infrastructure.Database.Entities
{
    public class Conversation : BaseEntity
    {
        public ICollection<UserConversation> UserConversations { get; set; } = [];

        [NotMapped]
        public IReadOnlyCollection<User> Users =>
            UserConversations.Select(uc => uc.User).ToList();

        public List<Message> Messages { get; set; } = [];
    }
}
