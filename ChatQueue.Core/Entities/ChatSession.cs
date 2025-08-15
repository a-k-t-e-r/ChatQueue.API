using ChatQueue.Core.Abstractions;
using ChatQueue.Core.Enums;

namespace ChatQueue.Core.Entities;

public sealed class ChatSession : AggregateRoot
{
    public DateTimeOffset CreatedAt { get; private set; } = DateTimeOffset.UtcNow;
    public ChatStatus Status { get; private set; }
    public Guid? AssignedAgentId { get; private set; }
    public DateTimeOffset LastPolledAt { get; private set; }

    public ChatSession()
    {
        Status = ChatStatus.Queued;
        LastPolledAt = DateTimeOffset.UtcNow;
    }

    public void Assign(Guid agentId)
    {
        AssignedAgentId = agentId;
        Status = ChatStatus.Active;
    }

    public void MarkRefused() => Status = ChatStatus.Refused;
    public void MarkCompleted() => Status = ChatStatus.Completed;
    public void MarkInactive() => Status = ChatStatus.Inactive;
    public void TouchPoll(DateTimeOffset now) => LastPolledAt = now;
}