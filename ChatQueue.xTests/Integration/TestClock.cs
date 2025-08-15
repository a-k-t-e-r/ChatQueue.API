using ChatQueue.Application.Abstractions;

namespace ChatQueue.xTests.Integration;

public sealed class TestClock : IDateTimeProvider
{
    private DateTimeOffset _now;

    public TestClock(DateTimeOffset start)
    {
        _now = start;
    }

    public DateTimeOffset UtcNow => _now;

    public void Advance(TimeSpan ts) => _now = _now.Add(ts);

    public void Set(DateTimeOffset t) => _now = t;
}