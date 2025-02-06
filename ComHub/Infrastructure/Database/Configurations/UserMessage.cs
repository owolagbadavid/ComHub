using ComHub.Infrastructure.Database.Entities;
using ComHub.Infrastructure.Database.Entities.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ComHub.Infrastructure.Database.Configurations;

internal class UserMessageConfiguration : IEntityTypeConfiguration<UserMessage>
{
    public void Configure(EntityTypeBuilder<UserMessage> builder)
    {
        builder.ToTable(Table.UserMessage);

        builder.HasKey(x => x.Id);
        builder.HasAlternateKey(x => new { x.MessageId, x.UserId });
        builder.Property(x => x.Status).HasDefaultValue(MessageStatus.Sent);
    }
}
