using ChatQueue.Core.Entities;

namespace ChatQueue.Application.Abstractions;

public interface IChatRepository
{
    ChatSession Add(ChatSession session);
    ChatSession? Get(Guid id);
    IEnumerable<ChatSession> GetQueued();
    IEnumerable<ChatSession> GetByAgent(Guid agentId);
    void Update(ChatSession session);
}

public interface ITeamRepository
{
    IEnumerable<Team> GetActiveTeams(DateTimeOffset now);
    Team GetPrimaryTeam(DateTimeOffset now);
    Team GetOverflowTeam();
    Agent? GetAgentById(Guid agentId);
}

public interface IQueueRepository
{
    int Count { get; }
    void Enqueue(Guid chatId);
    Guid? Dequeue();
    bool Contains(Guid id);
    IEnumerable<Guid> Snapshot();
}