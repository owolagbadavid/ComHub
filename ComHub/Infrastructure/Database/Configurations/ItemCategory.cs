namespace ComHub.Infrastructure.Database.Configurations;

using ComHub.Infrastructure.Database.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

internal class ItemCategoryConfiguration : IEntityTypeConfiguration<ItemCategory>
{
    public void Configure(EntityTypeBuilder<ItemCategory> builder)
    {
        builder.ToTable("ItemCategories");

        // relationship table between Item and Category
        builder.HasKey(itemCategory => new { itemCategory.ItemId, itemCategory.CategoryId });
    }
}
