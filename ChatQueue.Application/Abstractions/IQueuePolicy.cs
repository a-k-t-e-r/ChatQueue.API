namespace ChatQueue.Application.Abstractions;

public interface IQueuePolicy
{
    bool IsOfficeHours(DateTimeOffset now);
    bool OverflowAllowed(DateTimeOffset now);
}