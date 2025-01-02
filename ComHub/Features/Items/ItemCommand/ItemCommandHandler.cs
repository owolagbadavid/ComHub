namespace ComHub.Features.Items.ItemCommand;

using Api.Db;
using ComHub.Infrastructure.Database.Entities;
using ComHub.Shared.Exceptions;
using ComHub.Shared.Services.Utils;
using Microsoft.EntityFrameworkCore;

public class ItemCommandHandler(AppDbContext dbContext, IUserContext userContext)
{
    private readonly AppDbContext _dbContext = dbContext;

    public async Task<int> AddEditItem(CreateItemRequest request, int id = default)
    {
        var user =
            await _dbContext.Users.FindAsync(userContext.UserId)
            ?? throw new NotFoundException("User not found");

        Item? item;

        if (id != default)
        {
            item =
                await _dbContext.Items.FirstOrDefaultAsync(x => x.Id == id && x.OwnerId == user.Id)
                ?? throw new NotFoundException("Item not found");

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
            };

            await _dbContext.Items.AddAsync(item);
        }

        await _dbContext.SaveChangesAsync();

        return item.Id;
    }
}
