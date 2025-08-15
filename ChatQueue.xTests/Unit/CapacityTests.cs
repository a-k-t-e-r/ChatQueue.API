using ChatQueue.Core.Entities;
using ChatQueue.Core.Enums;
using ChatQueue.Core.Services;
using FluentAssertions;

namespace ChatQueue.xTests.Unit;

public class CapacityTests
{
    [Fact]
    public void TeamCapacity_ExampleFromSpec()
    {
        var team = new Team("Spec");
        team.AddAgent(new Agent("A", Seniority.MidLevel));
        team.AddAgent(new Agent("B", Seniority.MidLevel));
        team.AddAgent(new Agent("C", Seniority.Junior));

        var cap = CapacityCalculator.TeamCapacity(team);
        cap.Should().Be(16);
        CapacityCalculator.MaxQueueLength(team).Should().Be(24);
    }
}