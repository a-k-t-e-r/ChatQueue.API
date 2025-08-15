using ChatQueue.Application.Abstractions;
using ChatQueue.Core.Entities;
using ChatQueue.Core.Enums;

namespace ChatQueue.Infrastructure.Repositories;

public sealed class InMemoryTeamRepository : ITeamRepository
{
    private readonly Team _teamA;
    private readonly Team _teamB;
    private readonly Team _teamC;
    private readonly Team _overflow;

    public InMemoryTeamRepository()
    {
        _teamA = new Team("Team A");
        _teamA.AddAgent(new Agent("Akter TL", Seniority.TeamLead));
        _teamA.AddAgent(new Agent("Nasrin Mid", Seniority.MidLevel));
        _teamA.AddAgent(new Agent("Tamanna Mid", Seniority.MidLevel));
        _teamA.AddAgent(new Agent("Molly Jr", Seniority.Junior));

        _teamB = new Team("Team B");
        _teamB.AddAgent(new Agent("Akbar Sr", Seniority.Senior));
        _teamB.AddAgent(new Agent("Mia Mid", Seniority.MidLevel));
        _teamB.AddAgent(new Agent("Sia Jr", Seniority.Junior));
        _teamB.AddAgent(new Agent("Lia Jr", Seniority.Junior));

        _teamC = new Team("Team C");
        _teamC.AddAgent(new Agent("Nina Mid", Seniority.MidLevel));
        _teamC.AddAgent(new Agent("Omar Mid", Seniority.MidLevel));

        _overflow = new Team("Overflow", isOverflow: true);
        for (int i = 1; i <= 6; i++)
            _overflow.AddAgent(new Agent($"Overflow-{i}", Seniority.Junior));
    }

    public IEnumerable<Team> GetActiveTeams(DateTimeOffset now)
    {
        var hour = now.UtcDateTime.Hour; 
        if (hour >= 6 && hour < 14)     // Shift 1
            return [_teamA, _teamB];
        
        if (hour >= 14 && hour < 22)    // Shift 2
            return [_teamB];
        
        return [_teamC];    // Night shift
    }

    public Team GetPrimaryTeam(DateTimeOffset now) => GetActiveTeams(now).First();

    public Team GetOverflowTeam() => _overflow;

    public Agent? GetAgentById(Guid agentId)
    {
        var allTeams = new[] { _teamA, _teamB, _teamC, _overflow };
        foreach (var t in allTeams)
        {
            var agent = t.Agents.FirstOrDefault(a => a.Id == agentId);
            if (agent != null)
                return agent;
        }
        return null;
    }
}