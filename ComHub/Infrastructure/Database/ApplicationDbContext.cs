namespace Api.Db;

using System.Linq.Expressions;
using ComHub.Infrastructure.Database.Entities;
using ComHub.Infrastructure.Database.Entities.Enums;
using Microsoft.EntityFrameworkCore;

public class AppDbContext(DbContextOptions options) : DbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasPostgresEnum<UserRole>();
        modelBuilder.HasPostgresEnum<ItemStatus>();

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(BaseEntity).IsAssignableFrom(entityType.ClrType))
            {
                modelBuilder
                    .Entity(entityType.ClrType)
                    .HasQueryFilter(ConvertFilterExpression(entityType.ClrType));
            }
        }
        modelBuilder.Entity<ItemCategory>().HasQueryFilter(ic => !ic.Category.IsDeleted);
        modelBuilder.Entity<ItemCategory>().HasQueryFilter(ic => !ic.Item.IsDeleted);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }

    private static LambdaExpression ConvertFilterExpression(Type entityType)
    {
        var parameter = Expression.Parameter(entityType, "e");
        var property = Expression.Property(parameter, nameof(BaseEntity.IsDeleted));
        var comparison = Expression.Equal(property, Expression.Constant(false));
        return Expression.Lambda(comparison, parameter);
    }

    public override int SaveChanges()
    {
        var entries = ChangeTracker.Entries<BaseEntity>();
        var currentTime = DateTime.UtcNow;

        foreach (var entry in entries)
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAt = currentTime;
                entry.Entity.UpdatedAt = currentTime;
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAt = currentTime;
            }
        }

        return base.SaveChanges();
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var entries = ChangeTracker.Entries<BaseEntity>();
        var currentTime = DateTime.UtcNow;

        foreach (var entry in entries)
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAt = currentTime;
                entry.Entity.UpdatedAt = currentTime;
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAt = currentTime;
            }
        }

        return await base.SaveChangesAsync(cancellationToken);
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Profile> Profiles => Set<Profile>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Item> Items => Set<Item>();
    public DbSet<ItemCategory> ItemCategories => Set<ItemCategory>();
    public DbSet<ItemImage> ItemImages => Set<ItemImage>();
}
