namespace Api.Db;

using System.Linq.Expressions;
using System.Text.RegularExpressions;
using ComHub.Infrastructure.Database.Entities;
using ComHub.Infrastructure.Database.Entities.Enums;
using ComHub.Shared.Exceptions;
using Microsoft.EntityFrameworkCore;
using Npgsql;

public partial class AppDbContext(DbContextOptions options) : DbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasPostgresEnum<UserRole>();
        modelBuilder.HasPostgresEnum<ItemStatus>();
        modelBuilder.HasPostgresEnum<UserStatus>();
        modelBuilder.HasPostgresEnum<MessageStatus>();

        foreach (
            var property in modelBuilder
                .Model.GetEntityTypes()
                .SelectMany(x => x.GetProperties())
                .Where(x => x.ClrType == typeof(decimal) || x.ClrType == typeof(decimal?))
        )
        {
            property.SetColumnType("decimal(18,4)");
        }

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(BaseEntity).IsAssignableFrom(entityType.ClrType))
            {
                modelBuilder
                    .Entity(entityType.ClrType)
                    .HasQueryFilter(ConvertFilterExpression(entityType.ClrType));
            }

            foreach (var property in entityType.GetProperties())
            {
                var originalName = property.GetColumnName();
                var snakeCaseName = ToSnakeCase(originalName);
                property.SetColumnName(snakeCaseName);
            }
        }

        modelBuilder.Entity<ItemCategory>().HasQueryFilter(ic => !ic.Category.IsDeleted);
        modelBuilder.Entity<ItemCategory>().HasQueryFilter(ic => !ic.Item.IsDeleted);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }

    private static string ToSnakeCase(string input)
    {
        if (string.IsNullOrEmpty(input))
            return input;

        return MyRegex().Replace(input, "$1_$2").ToLower();
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

        try
        {
            return base.SaveChanges();
        }
        catch (DbUpdateException ex)
        {
            HandleDBException(ex);
            throw;
        }
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

        try
        {
            return await base.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex)
        {
            HandleDBException(ex);
            throw;
        }
    }

    // Updated regex for PostgreSQL foreign key constraint violations
    [GeneratedRegex(@"violates foreign key constraint ""(.+?)""", RegexOptions.Compiled)]
    private static partial Regex ForeignKeyConstraintRegex();

    // Updated regex for PostgreSQL unique constraint violations
    [GeneratedRegex(@"violates unique constraint ""(.+?)""", RegexOptions.Compiled)]
    private static partial Regex UniqueConstraintRegex();

    private static void HandleDBException(DbUpdateException ex)
    {
        if (ex.InnerException is PostgresException pgEx)
        {
            // PostgreSQL SQLSTATE error codes
            const string ForeignKeyViolationCode = "23503"; // Foreign key violation (includes restricted delete)
            const string UniqueViolationCode = "23505"; // Unique constraint violation

            if (pgEx.SqlState == ForeignKeyViolationCode)
            {
                string errorMessage = pgEx.Message.ToLower();

                // Handling general foreign key violation (invalid reference)
                var match = ForeignKeyConstraintRegex().Match(pgEx.Message);
                if (match.Success)
                {
                    string? constraintName = match.Groups[1].Value;
                    string? columnName = constraintName
                        ?.Replace("FK_", "")
                        .Split('_')
                        .LastOrDefault();

                    throw new BadRequestException(
                        $"{columnName ?? "A related entity"} does not exist or is invalid"
                    );
                }
            }
            else if (pgEx.SqlState == UniqueViolationCode)
            {
                // Handling unique constraint violation
                var match = UniqueConstraintRegex().Match(pgEx.Message);
                if (match.Success)
                {
                    string? constraintName = match.Groups[1].Value;
                    string? columnName = constraintName
                        ?.Replace("UQ_", "")
                        ?.Replace("IX_", "")
                        .Split('_')
                        .LastOrDefault();

                    throw new BadRequestException(
                        $"{columnName ?? "The entity"} must be unique but a duplicate value was found"
                    );
                }
            }
        }
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Profile> Profiles => Set<Profile>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Item> Items => Set<Item>();
    public DbSet<ItemCategory> ItemCategories => Set<ItemCategory>();
    public DbSet<ItemImage> ItemImages => Set<ItemImage>();
    public DbSet<UserMessage> UsersMessages => Set<UserMessage>();
    public DbSet<Message> Messages => Set<Message>();
    public DbSet<Conversation> Conversations => Set<Conversation>();
    public DbSet<UserConversation> UsersConversations => Set<UserConversation>();

    [GeneratedRegex(@"([a-z0-9])([A-Z])")]
    private static partial Regex MyRegex();
}
