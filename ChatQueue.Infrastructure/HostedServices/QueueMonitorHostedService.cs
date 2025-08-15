using ChatQueue.Application.Abstractions;
using ChatQueue.Core.Enums;
using Microsoft.Extensions.Hosting;

namespace ChatQueue.Infrastructure.HostedServices;

public sealed class QueueMonitorHostedService : BackgroundService
{
    private readonly IChatRepository _chats;
    private readonly IDateTimeProvider _clock;

    public QueueMonitorHostedService(IChatRepository chats, IDateTimeProvider clock)
    {
        _chats = chats;
        _clock = clock;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var now = _clock.UtcNow;
            foreach (var s in _chats.GetQueued())
            {
                // mark inactive once not received 3 poll requests ~ 3 seconds (assuming poll every 1s)
                if ((now - s.LastPolledAt).TotalSeconds >= 3 && s.Status == ChatStatus.Queued)
                {
                    s.MarkInactive();
                    _chats.Update(s);
                }
            }
            await Task.Delay(1000, stoppingToken);
        }
    }
}