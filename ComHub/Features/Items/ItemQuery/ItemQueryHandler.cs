namespace ComHub.Features.Items.ItemQuery;

using Api.Db;
using ComHub.Infrastructure.Database.Entities;
using ComHub.Shared.Exceptions;
using ComHub.Shared.Models;
using ComHub.Shared.Services.Utils;
using Microsoft.EntityFrameworkCore;

public class ItemQueryHandler(AppDbContext dbContext, IUserContext userContext)
{
    private readonly AppDbContext _dbContext = dbContext;

    public async Task<PaginationResponse<Item>> GetItems(PaginationRequest req)
    {
        var query = _dbContext.Items.AsQueryable();

        query = req.SortColumn switch
        {
            _ => string.Equals(req.SortOrder.Trim(), "asc")
                ? query.OrderBy(x => x.Id)
                : query.OrderByDescending(x => x.Id),
        };

        var count = await query.CountAsync();

        var items = await query
            .Skip((req.PageNumber - 1) * req.PageSize)
            .Take(req.PageSize)
            .ToListAsync();

        return new PaginationResponse<Item>
        {
            Items = items,
            Total = count,
            PageNumber = req.PageNumber,
            PageSize = req.PageSize,
        };
    }
}
