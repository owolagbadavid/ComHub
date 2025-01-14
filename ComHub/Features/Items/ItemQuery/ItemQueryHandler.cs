namespace ComHub.Features.Items.ItemQuery;

using Api.Db;
using AutoMapper;
using ComHub.Infrastructure.Database.Entities;
using ComHub.Shared.Exceptions;
using ComHub.Shared.Models;
using ComHub.Shared.Services.Utils;
using Microsoft.EntityFrameworkCore;

public class ItemQueryHandler(AppDbContext dbContext, IUserContext userContext, IMapper mapper)
{
    private readonly AppDbContext _dbContext = dbContext;

    public async Task<PaginationResponse<SearchItemModel>> GetItems(PaginationRequest req)
    {
        var query = _dbContext.Items.AsQueryable();

        query = req.SortColumn switch
        {
            _ => string.Equals(req.SortOrder.Trim(), "asc")
                ? query.OrderBy(x => x.Id)
                : query.OrderByDescending(x => x.Id),
        };

        var count = await query.CountAsync();

        var items = mapper.Map<List<SearchItemModel>>(
            await query.Skip((req.PageNumber - 1) * req.PageSize).Take(req.PageSize).ToListAsync()
        );

        return new PaginationResponse<SearchItemModel>
        {
            Items = items,
            Total = count,
            PageNumber = req.PageNumber,
            PageSize = req.PageSize,
        };
    }

    public async Task<ItemModel> GetItem(int id)
    {
        return mapper.Map<ItemModel>(await _dbContext.Items.FindAsync(id))
            ?? throw new NotFoundException("Item not found");
    }
}
