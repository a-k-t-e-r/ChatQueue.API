using MediatR;

namespace ChatQueue.Application.Chats.Commands;

public sealed record CreateChatSessionCommand() : IRequest<CreateChatSessionResult>;

public sealed record CreateChatSessionResult(Guid SessionId, string Status, string Message);