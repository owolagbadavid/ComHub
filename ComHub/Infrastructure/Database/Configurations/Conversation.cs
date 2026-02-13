using ComHub.Infrastructure.Database.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ComHub.Infrastructure.Database.Configurations;

internal class ConversationConfiguration : IEntityTypeConfiguration<Conversation>
{
    public void Configure(EntityTypeBuilder<Conversation> builder)
    {
        builder.ToTable(Table.Conversation);

        builder.HasKey(x => x.Id);

        builder
            .HasMany(c => c.UserConversations)
            .WithOne(uc => uc.Conversation)
            .HasForeignKey(uc => uc.ConversationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasMany(c => c.Messages)
            .WithOne(m => m.Conversation)
            .HasForeignKey(m => m.ConversationId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
