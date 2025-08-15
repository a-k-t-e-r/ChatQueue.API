using MediatR;

namespace ChatQueue.Application.Chats.Commands;

public sealed record PollChatSessionCommand(Guid SessionId) : IRequest<PollChatSessionResult>;

public sealed record PollChatSessionResult(Guid SessionId, string Status, string? AssignedAgentId);