using Api.Db;
using ComHub.Shared.Config;
using ComHub.Shared.Middlewares;
using ComHub.Shared.Services.Notifications.Email;
using MassTransit;
using Microsoft.EntityFrameworkCore;

// using FluentValidation;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddEnvironmentVariables();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.RegisterSwagger();

builder.Services.AddSharedServices(builder.Configuration);
builder.Services.RegisterHandlers();

var config =
    builder.Configuration.GetSection("AppConfig").Get<Config>()
    ?? throw new Exception("AppConfig is not configured");

builder.Services.RegisterAuth(config);

// builder.Services.AddValidatorsFromAssemblyContaining<Program>();

var connectionStrings = config.ConnectionStrings;

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionStrings.DefaultConnection)
);

builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = connectionStrings.RedisConnection;
    options.InstanceName = "ComHub_"; // Optional: Prefix for cache keys
});

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
builder.Services.AddAutoMapper(typeof(Program));

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(builder =>
    {
        builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
    });
});

var app = builder.Build();

app.UseCors();
app.UseMiddleware<GlobalExceptionHandlerMiddleware>();

var api = app.MapGroup("/api");
api.RegisterEndpoints();
app.UseHttpsRedirection();

app.RegisterSwagger();

app.Run();
