using ComHub.Shared.Exceptions;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace ComHub.Shared.Services.Utils;

public static class HelperService
{
    public static async Task HandleValidation<T>(AbstractValidator<T> validator, T request)
    {
        var result = await validator.ValidateAsync(request);
        if (!result.IsValid)
        {
            throw new BadRequestException(result.Errors[0].ErrorMessage);
        }
    }

    public static void HandleDBException(DbUpdateException ex)
    {
        if (ex.InnerException is PostgresException pgEx)
        {
            if (pgEx.SqlState == "23505")
            {
                string? columnName = pgEx.ConstraintName?.Replace("UQ_", "").Replace("IX_", ""); // Common naming conventions
                if (!string.IsNullOrEmpty(columnName) && columnName.Contains('_'))
                {
                    columnName = columnName[(columnName.LastIndexOf('_') + 1)..];
                }

                throw new BadRequestException($"{columnName ?? ""} Already exists".Trim());
            }
        }
    }

    public static bool BeAValidUrl(string? url)
    {
        return Uri.TryCreate(url, UriKind.Absolute, out Uri? uriResult)
            && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
    }

    public static string GenerateRandomString(int length)
    {
        var random = new Random();
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        return new string(
            [.. Enumerable.Repeat(chars, length).Select(s => s[random.Next(s.Length)])]
        );
    }
}
