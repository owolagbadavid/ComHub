namespace ComHub.Infrastructure.Database.Entities;

public class ItemImage : BaseEntity
{
    public required string Url { get; set; }
    public int ItemId { get; set; }
    public required Item Item { get; set; }
    public bool IsMain { get; set; } = false;
}
