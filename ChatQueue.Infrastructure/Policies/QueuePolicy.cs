using ChatQueue.Application.Abstractions;

namespace ChatQueue.Infrastructure.Policies;

public sealed class QueuePolicy : IQueuePolicy
{
    private readonly TimeSpan _officeStart;
    private readonly TimeSpan _officeEnd;

    public QueuePolicy(TimeSpan? officeStart = null, TimeSpan? officeEnd = null)
    {
        _officeStart = officeStart ?? new TimeSpan(9, 0, 0);
        _officeEnd = officeEnd ?? new TimeSpan(17, 0, 0);
    }

    public bool IsOfficeHours(DateTimeOffset now)
        => now.TimeOfDay >= _officeStart && now.TimeOfDay < _officeEnd;

    public bool OverflowAllowed(DateTimeOffset now) => IsOfficeHours(now);
}