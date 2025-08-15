using ChatQueue.Core.Abstractions;

namespace ChatQueue.Core.Entities;

public sealed class Team(string name, bool isOverflow = false) : AggregateRoot
{
    private readonly List<Agent> _agents = new();
    public string Name { get; } = name;
    public bool IsOverflow { get; } = isOverflow;

    public IReadOnlyList<Agent> Agents => _agents;

    public void AddAgent(Agent agent) => _agents.Add(agent);
}