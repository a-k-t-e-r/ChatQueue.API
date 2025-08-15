using ChatQueue.Core.Entities;

namespace ChatQueue.Core.Services;

public static class CapacityCalculator
{
    public static int TeamCapacity(Team team)
    {
        return team.Agents.Sum(a => a.EffectiveCapacity);
    }

    public static int MaxQueueLength(Team team) => (int)Math.Floor(TeamCapacity(team) * 1.5m);
}