namespace ComHub.Infrastructure.Database.Configurations;

using ComHub.Infrastructure.Database.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

internal class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.ToTable(Table.Category);

        builder.HasKey(category => category.Id);
        builder.Property(category => category.Id).ValueGeneratedOnAdd();

        builder.Property(category => category.Name).HasMaxLength(50);

        // restrict deletion of categories that are still in use
        builder
            .HasMany(category => category.ItemCategories)
            .WithOne(itemCategory => itemCategory.Category)
            .HasForeignKey(itemCategory => itemCategory.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
