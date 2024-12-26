using System.Text.Json.Serialization;
using Api.Db;
using ComHub.Shared.Config;
using ComHub.Shared.Middlewares;
using ComHub.Shared.Services.Notifications.Email;
using MassTransit;
using Microsoft.EntityFrameworkCore;

// using FluentValidation;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddEnvironmentVariables();

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.RegisterSwagger();

builder.Services.AddHttpContextAccessor();

builder.Services.AddSharedServices();
builder.Services.RegisterHandlers();

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

builder.Services.RegisterAuth(jwtSettings);

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

app.UseCors("default");

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
