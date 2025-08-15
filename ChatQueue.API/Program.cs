using ChatQueue.Application.Abstractions;
using ChatQueue.Application.Chats.Commands;
using ChatQueue.Application.Services;
using ChatQueue.Infrastructure.HostedServices;
using ChatQueue.Infrastructure.Policies;
using ChatQueue.Infrastructure.Queues;
using ChatQueue.Infrastructure.Repositories;
using ChatQueue.Infrastructure.Time;
using Microsoft.AspNetCore.RateLimiting;
using System.Text.Json;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

/*** Add services to the container - START ***/

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers();

// Configure logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();
builder.Logging.AddConfiguration(builder.Configuration.GetSection("Logging"));
// For production
builder.Logging.AddJsonConsole(options =>
{
    options.IncludeScopes = true;
    options.TimestampFormat = "yyyy-MM-dd HH:mm:ss.fff";
    options.JsonWriterOptions = new JsonWriterOptions { Indented = true };
});

builder.Services.AddRateLimiter(limiterOptions =>
{
    limiterOptions.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    limiterOptions.AddFixedWindowLimiter("fixed", options =>
    {
        options.PermitLimit = 100;
        options.Window = TimeSpan.FromSeconds(10);
        options.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        options.QueueLimit = 10;
    });
});

builder.Services.AddMemoryCache();

builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(CreateChatSessionCommand).Assembly));

builder.Services.AddSingleton<ICacheService, MemoryCacheService>();
builder.Services.AddSingleton<IChatRepository, InMemoryChatRepository>();
builder.Services.AddSingleton<ITeamRepository, InMemoryTeamRepository>();
builder.Services.AddSingleton<IQueueRepository, InMemoryQueue>();
builder.Services.AddSingleton<IQueuePolicy>(_ => new QueuePolicy());
builder.Services.AddSingleton<IDateTimeProvider, SystemClock>();

builder.Services.AddHostedService<QueueMonitorHostedService>();
builder.Services.AddHostedService<AssignmentHostedService>();

/*** Add services to the container - END ***/

var app = builder.Build();

/*** Configure the HTTP request pipeline - START ***/
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.MapControllers();

app.UseRateLimiter();

app.Run();

/*** Configure the HTTP request pipeline - END ***/