using ChatQueue.Application.Abstractions;
using ChatQueue.Application.Chats.Commands;
using ChatQueue.Infrastructure.HostedServices;
using ChatQueue.Infrastructure.Policies;
using ChatQueue.Infrastructure.Queues;
using ChatQueue.Infrastructure.Repositories;
using ChatQueue.Infrastructure.Time;

var builder = WebApplication.CreateBuilder(args);

/*** Add services to the container - START ***/

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers();

builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(CreateChatSessionCommand).Assembly));

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

app.Run();

/*** Configure the HTTP request pipeline - END ***/