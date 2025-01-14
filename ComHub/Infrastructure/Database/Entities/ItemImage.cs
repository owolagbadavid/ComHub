namespace ComHub.Infrastructure.Database.Entities;

public class ItemImage : BaseEntity
{
    public required string Url { get; set; }

    public required int ItemId { get; set; }

    public required Item Item { get; set; }
    public required bool IsMain { get; set; }
}
