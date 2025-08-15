using ChatQueue.Application.Abstractions;

namespace ChatQueue.Infrastructure.Time;

public sealed class SystemClock : IDateTimeProvider
{
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}