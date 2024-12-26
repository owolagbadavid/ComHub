using System.Reflection;
using System.Text;
using System.Text.Json.Serialization;
using Api.Db;
using ComHub.Features.Account.Auth;
using ComHub.Features.Account.Profile;
using ComHub.Shared.Config;
using ComHub.Shared.Exceptions;
using ComHub.Shared.Middlewares;
using ComHub.Shared.Services.Notifications.Email;
using ComHub.Shared.Services.Utils;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

// using FluentValidation;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddEnvironmentVariables();

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition(
        "Bearer",
        new Microsoft.OpenApi.Models.OpenApiSecurityScheme
        {
            Name = "Authorization",
            Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
            Scheme = "Bearer",
            BearerFormat = "JWT",
            In = Microsoft.OpenApi.Models.ParameterLocation.Header,
            Description = "Enter 'Bearer' [space] and then your valid JWT token.",
        }
    );

    options.AddSecurityRequirement(
        new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
        {
            {
                new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                {
                    Reference = new Microsoft.OpenApi.Models.OpenApiReference
                    {
                        Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                        Id = "Bearer",
                    },
                },
                Array.Empty<string>()
            },
        }
    );
});
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<ISecurityService, SecurityService>();
builder.Services.AddScoped<IUserContext, UserContext>();
builder.Services.AddScoped<AuthHandler>();
builder.Services.AddScoped<ProfileHandler>();

builder.Services.Configure<Microsoft.AspNetCore.Http.Json.JsonOptions>(options =>
{
    options.SerializerOptions.PropertyNameCaseInsensitive = true;
    options.SerializerOptions.AllowTrailingCommas = true;
    options.SerializerOptions.NumberHandling = JsonNumberHandling.AllowReadingFromString;
    options.SerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault;
});

builder.Services.Configure<Config>(builder.Configuration.GetSection("AppConfig"));

var config =
    builder.Configuration.GetSection("AppConfig").Get<Config>()
    ?? throw new Exception("AppConfig is not configured");

var jwtSettings = config.JwtSettings;

builder
    .Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(
        (options) =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtSettings.Issuer,
                ValidAudience = jwtSettings.Audience,
                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(jwtSettings.Secret)
                ),
            };
            options.Events = new JwtBearerEvents
            {
                OnAuthenticationFailed = context =>
                {
                    throw new UnauthorizedException("Invalid token");
                },

                OnForbidden = context =>
                {
                    throw new ForbiddenException("Forbidden access");
                },

                OnChallenge = context =>
                {
                    if (string.IsNullOrEmpty(context.Request.Headers["Authorization"]))
                    {
                        throw new UnauthorizedException("Authorization header is missing");
                    }

                    return Task.CompletedTask;
                },
            };
        }
    );

builder.Services.AddAuthorization();

// builder.Services.AddValidatorsFromAssemblyContaining<Program>();

var connectionStrings = config.ConnectionStrings;

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionStrings.DefaultConnection)
);

builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<EmailNotificationConsumer>();

    x.UsingInMemory(
        (context, cfg) =>
        {
            cfg.ConfigureEndpoints(context);
        }
    );
});

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(builder =>
    {
        builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
    });
});

builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = connectionStrings.RedisConnection;
    options.InstanceName = "ComHub_"; // Optional: Prefix for cache keys
});

var app = builder.Build();

app.UseCors(builder => builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());

app.UseMiddleware<GlobalExceptionHandlerMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

var api = app.MapGroup("/api");
api.RegisterEndpoints();
app.UseHttpsRedirection();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        // ! sus behavior
        c.ConfigObject.AdditionalItems.Add("persistAuthorization", true);
    });
}

app.Run();
