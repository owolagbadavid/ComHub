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
    }
}
