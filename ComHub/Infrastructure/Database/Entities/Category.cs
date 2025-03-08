namespace ComHub.Infrastructure.Database.Entities;

public class Category : BaseEntity
{
    public required string Name { get; set; }
    public required string Description { get; set; }
    public List<ItemCategory>? ItemCategories { get; set; }
}
