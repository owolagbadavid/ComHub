using System.Text.Json.Serialization;

namespace ComHub.Infrastructure.Database.Entities;

public class Category : BaseEntity
{
    public required string Name { get; set; }

    public required string Description { get; set; }

    [JsonIgnore]
    public List<ItemCategory>? ItemCategories { get; set; }
}
