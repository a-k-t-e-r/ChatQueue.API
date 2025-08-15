using ChatQueue.Core.Abstractions;
using ChatQueue.Core.Enums;
using ChatQueue.Core.Values;

namespace ChatQueue.Core.Entities;

public sealed class Agent(string name, Seniority seniority) : Entity
{
    private long _currentChats;
    public long CurrentChats => Volatile.Read(ref _currentChats);
    public string Name { get; private set; } = name;
    public Seniority Seniority { get; private set; } = seniority;
    public int MaxConcurrency { get; } = 10;

    public bool IsOnShift(DateTimeOffset now, (TimeSpan start, TimeSpan end) shift) =>
        now.TimeOfDay >= shift.start && now.TimeOfDay < shift.end;

    public int EffectiveCapacity => (int)Math.Floor(MaxConcurrency * (double)SeniorityMultiplier.For(Seniority).Multiplier);

    public bool CanAcceptMore() => CurrentChats < EffectiveCapacity;

    public void AssignChat()
    {
        var newVal = Interlocked.Increment(ref _currentChats);
        if (newVal > EffectiveCapacity)
        {
            Interlocked.Decrement(ref _currentChats);
            throw new InvalidOperationException("Agent capacity reached.");
        }
    }

    public void ReleaseChat()
    {
        if (Interlocked.Read(ref _currentChats) > 0)
            Interlocked.Decrement(ref _currentChats);
    }
}