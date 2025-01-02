using System.ComponentModel.DataAnnotations;
using ComHub.Infrastructure.Database.Entities.Enums;

namespace ComHub.Infrastructure.Database.Entities;

public class Item : BaseEntity
{
    public required string Name { get; set; }

    public required string Description { get; set; }

    public required decimal Price { get; set; }

    [Range(1, int.MaxValue)]
    public required int Quantity { get; set; }

    public List<ItemImage>? Images { get; set; }

    public required string Brand { get; set; }

    public required User Owner { get; set; }

    public int OwnerId { get; set; }

    public List<ItemCategory> ItemCategories { get; set; } = [];

    public ItemStatus Status { get; set; } = ItemStatus.Pending; // Optional: e.g., "Sold", "Available"

    // public required List<string> Colors { get; set; }

    // public required List<string> Sizes { get; set; }
}
