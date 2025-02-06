namespace ComHub.Infrastructure.Database.Configurations;

using ComHub.Infrastructure.Database.Entities;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

internal class ProfileConfiguration : IEntityTypeConfiguration<Profile>
{
    public void Configure(EntityTypeBuilder<Profile> builder)
    {
        builder.ToTable(Table.Profile);

        builder.HasKey(profile => profile.Id);
        builder.Property(profile => profile.Id).ValueGeneratedOnAdd();

        builder.Property(profile => profile.FirstName).HasMaxLength(50);
        builder.Property(profile => profile.LastName).HasMaxLength(50);
    }
}
