using Api.Db;
using ComHub.Shared.Config;
using ComHub.Shared.Middlewares;
using ComHub.Shared.Services.Notifications.Email;
using MassTransit;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using SignalRChat.Hubs;

// using FluentValidation;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddEnvironmentVariables();

builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 10 * 1024 * 1024; // 10MB limit
});

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.RegisterSwagger();
var config =
    builder.Configuration.GetSection("AppConfig").Get<Config>()
    ?? throw new Exception("AppConfig is not configured");

builder.Services.AddSharedServices(builder.Configuration, config);
builder.Services.RegisterHandlers();

builder.Services.RegisterAuth(config);
builder.Services.AddSingleton(config);

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
builder.Services.AddSingleton<CustomFilter>();
builder
    .Services.AddSignalR(opts =>
    {
        opts.AddFilter<CustomFilter>();
    })
    .AddHubOptions<TestHub>(options =>
    {
        options.EnableDetailedErrors = true;
    });

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
var hub = app.MapGroup("/hub");

hub.MapHub<TestHub>("/test");
api.RegisterEndpoints();

app.UseHttpsRedirection();

app.RegisterSwagger();

app.Run();
