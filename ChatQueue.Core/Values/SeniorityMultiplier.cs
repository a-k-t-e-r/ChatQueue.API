using ChatQueue.Core.Abstractions;
using ChatQueue.Core.Enums;

namespace ChatQueue.Core.Values;

public sealed class SeniorityMultiplier : ValueObject
{
    public Seniority Seniority { get; }
    public decimal Multiplier { get; }

    private SeniorityMultiplier(Seniority seniority, decimal multiplier)
    {
        Seniority = seniority;
        Multiplier = multiplier;
    }

    public static SeniorityMultiplier For(Seniority seniority) =>
    seniority switch
    {
        Seniority.Junior => new SeniorityMultiplier(seniority, 0.4m),
        Seniority.MidLevel => new SeniorityMultiplier(seniority, 0.6m),
        Seniority.Senior => new SeniorityMultiplier(seniority, 0.8m),
        Seniority.TeamLead => new SeniorityMultiplier(seniority, 0.5m),
        _ => throw new ArgumentOutOfRangeException(nameof(seniority))
    };

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return (int)Seniority;
        yield return Multiplier;
    }
}