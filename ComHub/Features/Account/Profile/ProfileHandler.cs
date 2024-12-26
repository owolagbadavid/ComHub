namespace ComHub.Features.Account.Profile;

using Api.Db;
using ComHub.Infrastructure.Database.Entities;
using ComHub.Shared.Exceptions;
using MassTransit.Initializers;
using Microsoft.EntityFrameworkCore;

public class ProfileHandler(AppDbContext dbContext)
{
    private readonly AppDbContext _dbContext = dbContext;

    public async Task<ProfileModel> GetProfileAsync(int userId)
    {
        return await _dbContext
                .Profiles.FirstOrDefaultAsync(p => p.UserId == userId)
                .Select(p =>
                    p != null
                        ? new ProfileModel
                        {
                            FirstName = p.FirstName,
                            LastName = p.LastName,
                            ProfilePicture = p.ProfilePicture,
                            Bio = p.Bio,
                            Website = p.Website,
                            Location = p.Location,
                        }
                        : null
                ) ?? throw new NotFoundException("Profile not found");
    }

    public async Task UpdateProfileAsync(int userId, PutProfileRequest request)
    {
        var user =
            await _dbContext.Users.Include(u => u.Profile).FirstOrDefaultAsync(u => u.Id == userId)
            ?? throw new NotFoundException("User not found");

        var profile = user.Profile ?? new Profile { User = user, FirstName = request.FirstName };

        profile.FirstName = request.FirstName;
        profile.LastName = request.LastName;
        profile.ProfilePicture = request.ProfilePicture;
        profile.Bio = request.Bio;
        profile.Website = request.Website;
        profile.Location = request.Location;

        if (user.Profile == null)
        {
            await _dbContext.Profiles.AddAsync(profile);
        }
        else
        {
            _dbContext.Profiles.Update(profile);
        }

        await _dbContext.SaveChangesAsync();
    }
}
