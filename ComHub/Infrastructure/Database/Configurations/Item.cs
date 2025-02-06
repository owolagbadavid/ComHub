namespace ComHub.Infrastructure.Database.Configurations;

using ComHub.Infrastructure.Database.Entities;
using ComHub.Infrastructure.Database.Entities.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class ItemConfiguration : IEntityTypeConfiguration<Item>
{
    public void Configure(EntityTypeBuilder<Item> builder)
    {
        builder.ToTable(Table.Item);

        builder.HasKey(item => item.Id);
        builder.Property(item => item.Id).ValueGeneratedOnAdd();
        builder.Property(item => item.Status).HasDefaultValue(ItemStatus.Pending);
        builder.Property(item => item.Name).HasMaxLength(50);
        builder.Property(item => item.Description).HasMaxLength(500);
        builder.Property(item => item.Price).HasColumnType("decimal(18, 2)");
    }
}
