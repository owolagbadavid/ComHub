namespace ComHub.Infrastructure.Database.Configurations;

using ComHub.Infrastructure.Database.Entities;
using ComHub.Infrastructure.Database.Entities.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

internal class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable(Table.User);

        builder.HasKey(user => user.Id);
        builder.Property(user => user.Id).ValueGeneratedOnAdd();
        builder.Property(user => user.Role).HasDefaultValue(UserRole.User);
        builder.HasIndex(user => user.Email).IsUnique();
        builder
            .HasOne(user => user.Profile)
            .WithOne(profile => profile.User)
            .HasForeignKey<Profile>(profile => profile.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
