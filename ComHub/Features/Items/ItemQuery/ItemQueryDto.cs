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
    public List<string> Categories { get; set; } = [];
    public List<string> Images { get; set; } = [];
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
    public List<string> Categories { get; set; } = [];
    public required string Image { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class CategoryModel
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
}

sealed class MappingProfile : AutoMapper.Profile
{
    public MappingProfile()
    {
        CreateMap<Item, ItemModel>()
            .ForMember(
                dest => dest.Categories,
                opt => opt.MapFrom(src => src.ItemCategories.Select(c => c.Category.Name).ToList())
            )
            .ForMember(
                dest => dest.Images,
                opt =>
                    opt.MapFrom(src =>
                        src.Images != null
                            ? src.Images.Select(i => i.Url).ToList()
                            : new List<string>()
                    )
            );

        CreateMap<Item, SearchItemModel>()
            .ForMember(
                dest => dest.Categories,
                opt => opt.MapFrom(src => src.ItemCategories.Select(c => c.Category.Name).ToList())
            )
            .ForMember(
                dest => dest.Image,
                opt =>
                    opt.MapFrom(src =>
                        src.Images != null && src.Images.Any(i => i.IsMain)
                            ? src.Images.First(i => i.IsMain).Url
                            : string.Empty
                    )
            );

        CreateMap<Category, CategoryModel>();
    }
}
