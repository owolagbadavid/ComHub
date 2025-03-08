namespace ComHub.Features.Items;

using ComHub.Features.Items.ItemQuery;
using ComHub.Infrastructure.Database.Entities;

sealed class ItemMappingProfile : AutoMapper.Profile
{
    public ItemMappingProfile()
    {
        CreateMap<Item, ItemModel>()
            .ForMember(
                dest => dest.Categories,
                opt => opt.MapFrom(src => src.ItemCategories.Select(ic => ic.Category).ToList())
            )
            .ForMember(dest => dest.Images, opt => opt.MapFrom(src => src.Images.ToList()));

        CreateMap<Item, SearchItemModel>()
            .ForMember(
                dest => dest.Categories,
                opt => opt.MapFrom(src => src.ItemCategories.Select(ic => ic.Category).ToList())
            )
            .ForMember(
                dest => dest.Image,
                opt =>
                    opt.MapFrom(src =>
                        src.Images != null && src.Images.Any(i => i.IsMain)
                            ? src.Images.First(i => i.IsMain)
                            : null
                    )
            );

        CreateMap<Category, CategoryModel>().ReverseMap();
        CreateMap<ItemImage, ImageModel>().ReverseMap();
    }
}
