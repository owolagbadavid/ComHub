namespace ComHub.Features.Items.ItemCommand;

using Api.Db;
using ComHub.Infrastructure.Cloud;
using ComHub.Infrastructure.Database.Entities;
using ComHub.Infrastructure.Database.Entities.Enums;
using ComHub.Shared.Exceptions;
using ComHub.Shared.Services.Utils;
using Microsoft.EntityFrameworkCore;

public class ItemCommandHandler(
    AppDbContext dbContext,
    IUserContext userContext,
    ICloudStorage cloudStorage
)
{
    private readonly AppDbContext _dbContext = dbContext;

    public async Task<CreatedWithUrls> AddEditItem(
        CreateItemRequest request,
        int id = default,
        CancellationToken ct = default
    )
    {
        var user =
            await _dbContext.Users.FindAsync([userContext.UserId], cancellationToken: ct)
            ?? throw new NotFoundException("User not found");

        Item? item;

        List<string> imageUrls = [];

        if (id != default)
        {
            item =
                await _dbContext
                    .Items.Include(x => x.Images)
                    .FirstOrDefaultAsync(
                        x => x.Id == id && x.OwnerId == user.Id,
                        cancellationToken: ct
                    ) ?? throw new NotFoundException("Item not found");

            item.Name = request.Name.Trim();
            item.Description = request.Description.Trim();
            item.Price = request.Price;
            item.Quantity = request.Quantity;
            item.Brand = request.Brand.Trim();

            foreach (var image in item.Images)
            {
                imageUrls.Add(image.Url);
                _dbContext.ItemImages.Remove(image);
            }

            _dbContext.Items.Update(item);
        }
        else
        {
            item = new Item
            {
                Name = request.Name.Trim(),
                Description = request.Description.Trim(),
                Price = request.Price,
                Quantity = request.Quantity,
                Brand = request.Brand.Trim(),
                Owner = user,
                Status = ItemStatus.Pending,
            };

            await _dbContext.Items.AddAsync(item, ct);
        }
        foreach (var categoryId in request.CategoryIds)
        {
            var category =
                await _dbContext.Categories.FindAsync([categoryId], cancellationToken: ct)
                ?? throw new NotFoundException("Category not found");

            var itemCategory = new ItemCategory { Item = item, Category = category };

            await _dbContext.ItemCategories.AddAsync(itemCategory, ct);
        }

        foreach (var i in Enumerable.Range(0, request.ImageCount))
        {
            var imageUrl = await cloudStorage.GeneratePresignedUrlAsync(
                HelperService.GenerateRandomString(8),
                ct: ct
            );

            var itemImage = new ItemImage { Item = item, Url = imageUrl };

            await _dbContext.ItemImages.AddAsync(itemImage, ct);
        }

        await _dbContext.SaveChangesAsync(ct);

        await Task.WhenAll(imageUrls.Select(url => cloudStorage.DeleteFileAsync(url, ct)));

        return new CreatedWithUrls { Id = item.Id, Urls = [.. item.Images.Select(x => x.Url)] };
    }
}
