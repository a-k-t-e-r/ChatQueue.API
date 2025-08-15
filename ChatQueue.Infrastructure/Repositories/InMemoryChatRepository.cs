using ChatQueue.Application.Abstractions;
using ChatQueue.Core.Entities;
using ChatQueue.Core.Enums;

namespace ChatQueue.Infrastructure.Repositories;

public sealed class InMemoryChatRepository : IChatRepository
{
    private readonly Dictionary<Guid, ChatSession> _store = new();

    public ChatSession Add(ChatSession session)
    {
        _store[session.Id] = session;

        return session;
    }

    public ChatSession? Get(Guid id) => _store.TryGetValue(id, out var s) ? s : null;

    public IEnumerable<ChatSession> GetQueued() => _store.Values.Where(s => s.Status == ChatStatus.Queued);

    public IEnumerable<ChatSession> GetByAgent(Guid agentId) => _store.Values.Where(s => s.AssignedAgentId == agentId);

    public void Update(ChatSession session) => _store[session.Id] = session;
}