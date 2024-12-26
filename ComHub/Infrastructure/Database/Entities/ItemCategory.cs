namespace ComHub.Infrastructure.Database.Entities;

public class ItemCategory
{
    public int ItemId { get; set; }
    public required Item Item { get; set; }

    public int CategoryId { get; set; }
    public required Category Category { get; set; }
}
