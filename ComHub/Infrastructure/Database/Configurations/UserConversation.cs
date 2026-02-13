using ComHub.Infrastructure.Database.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ComHub.Infrastructure.Database.Configurations;

internal class UserConversationConfiguration : IEntityTypeConfiguration<UserConversation>
{
    public void Configure(EntityTypeBuilder<UserConversation> builder)
    {
        builder.ToTable(Table.UserConversation);

        builder.HasKey(x => x.Id);
        builder.HasAlternateKey(x => new { x.ConversationId, x.UserId });

        builder
            .HasOne(uc => uc.User)
            .WithMany()
            .HasForeignKey(uc => uc.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasOne(uc => uc.Conversation)
            .WithMany(c => c.UserConversations)
            .HasForeignKey(uc => uc.ConversationId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
