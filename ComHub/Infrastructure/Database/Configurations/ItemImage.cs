namespace ComHub.Infrastructure.Database.Configurations;

using ComHub.Infrastructure.Database.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class ItemImageConfiguration : IEntityTypeConfiguration<ItemImage>
{
    public void Configure(EntityTypeBuilder<ItemImage> builder)
    {
        builder.ToTable("ItemImages");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id).ValueGeneratedOnAdd();

        builder.Property(e => e.Url).IsRequired().HasMaxLength(255);

        builder
            .HasOne(e => e.Item)
            .WithMany(e => e.Images)
            .HasForeignKey(e => e.ItemId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
