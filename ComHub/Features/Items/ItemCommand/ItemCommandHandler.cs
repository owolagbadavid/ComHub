namespace ComHub.Features.Items.ItemCommand;

using System.Collections.Concurrent;
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

    public async Task<int> AddEditItem(
        CreateItemRequest request,
        int id = default,
        CancellationToken ct = default
    )
    {
        var user =
            await _dbContext.Users.FindAsync([userContext.UserId], cancellationToken: ct)
            ?? throw new NotFoundException("User not found");

        Item? item;

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

        await _dbContext.SaveChangesAsync(ct);

        return item.Id;
    }

    public async Task DeleteItemImages(int id, List<int> imageIds, CancellationToken ct = default)
    {
        var item =
            await _dbContext
                .Items.Where(x => x.Id == id)
                .Select(x => new
                {
                    x.Id,
                    x.Name,
                    Images = x.Images.Where(img => imageIds.Contains(img.Id)).ToList(),
                })
                .FirstOrDefaultAsync(ct) ?? throw new NotFoundException("Item not found");

        foreach (var image in item.Images)
        {
            _dbContext.ItemImages.Remove(image);
        }

        await _dbContext.SaveChangesAsync(ct);

        await Task.WhenAll(
            item.Images.Select(image => cloudStorage.DeleteFileAsync(image.Url, ct))
        );

        await _dbContext.SaveChangesAsync(ct);
    }

    public async Task<int> AddItemImages(
        int id,
        IFormFileCollection images,
        CancellationToken ct = default
    )
    {
        var item =
            await _dbContext.Items.FindAsync([id], cancellationToken: ct)
            ?? throw new NotFoundException("Item not found");

        var imageUrls = new ConcurrentBag<string>();
        try
        {
            var options = new ParallelOptions { CancellationToken = ct };
            await Parallel.ForEachAsync(
                images,
                options,
                async (image, cancellationToken) =>
                {
                    string url = await cloudStorage.SaveFileAsync(
                        image,
                        HelperService.GenerateRandomString(6),
                        ct: cancellationToken
                    );

                    imageUrls.Add(url);
                }
            );

            foreach (var url in imageUrls)
            {
                var itemImage = new ItemImage { Item = item, Url = url };

                await _dbContext.ItemImages.AddAsync(itemImage, ct);
            }

            await _dbContext.SaveChangesAsync(ct);

            return item.Id;
        }
        catch (Exception)
        {
            //  Rollback
            _ = Task.Run(
                () => Task.WhenAll(imageUrls.Select(url => cloudStorage.DeleteFileAsync(url, ct))),
                CancellationToken.None
            );

            throw new Exception("Failed to upload images");
        }
    }

    public async Task AddEditCategories(List<Category> categories, CancellationToken ct = default)
    {
        var categoryNames = categories.Select(c => c.Name.ToLower()).ToHashSet();

        var existingCategories = await _dbContext
            .Categories.Where(c => categoryNames.Contains(c.Name.ToLower()))
            .ToListAsync(ct);

        var existingCategoryMap = existingCategories.ToDictionary(c => c.Name.ToLower());

        var newCategories = new List<Category>();

        foreach (var category in categories)
        {
            var lowerCaseName = category.Name.ToLower();

            if (existingCategoryMap.TryGetValue(lowerCaseName, out var existingCategory))
            {
                existingCategory.Name = category.Name;
                existingCategory.Description = category.Description;
            }
            else
            {
                newCategories.Add(category);
            }
        }

        if (newCategories.Count != 0)
            await _dbContext.Categories.AddRangeAsync(newCategories, ct);

        if (existingCategories.Count != 0)
            _dbContext.Categories.UpdateRange(existingCategories);

        await _dbContext.SaveChangesAsync(ct);
    }

    public async Task DeleteCategories(List<int> categoryIds, CancellationToken ct = default)
    {
        var categories = await _dbContext
            .Categories.Where(c => categoryIds.Contains(c.Id))
            .ToListAsync(ct);

        _dbContext.Categories.RemoveRange(categories);

        await _dbContext.SaveChangesAsync(ct);
    }
}
