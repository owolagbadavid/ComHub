using ComHub.Infrastructure.Database.Entities;

namespace ComHub.Features.Items.ItemQuery;

public class ItemModel
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string Description { get; set; }
    public required decimal Price { get; set; }
    public required int Quantity { get; set; }
    public string? Brand { get; set; }
    public required int OwnerId { get; set; }
    public List<CategoryModel> Categories { get; set; } = [];
    public List<ImageModel> Images { get; set; } = [];
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class SearchItemModel
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string Description { get; set; }
    public required decimal Price { get; set; }
    public required int Quantity { get; set; }
    public string? Brand { get; set; }
    public required int OwnerId { get; set; }
    public List<CategoryModel> Categories { get; set; } = [];
    public required ImageModel Image { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class ImageModel
{
    public int Id { get; set; }
    public required string Url { get; set; }
    public required bool IsMain { get; set; }
}

public class CategoryModel
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
}
