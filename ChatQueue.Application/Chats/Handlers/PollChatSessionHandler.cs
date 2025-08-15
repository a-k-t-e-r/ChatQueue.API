using ChatQueue.Application.Abstractions;
using ChatQueue.Application.Chats.Commands;
using MediatR;

namespace ChatQueue.Application.Chats.Handlers;

public sealed class PollChatSessionHandler(IChatRepository chats,
                                           IDateTimeProvider clock) : IRequestHandler<PollChatSessionCommand, PollChatSessionResult>
{
    private readonly IChatRepository _chats = chats ?? throw new ArgumentNullException(nameof(chats));
    private readonly IDateTimeProvider _clock = clock ?? throw new ArgumentNullException(nameof(clock));

    public Task<PollChatSessionResult> Handle(PollChatSessionCommand request, CancellationToken cancellationToken)
    {
        var session = _chats.Get(request.SessionId) ?? throw new KeyNotFoundException("Session not found");
        session.TouchPoll(_clock.UtcNow);
        _chats.Update(session);

        return Task.FromResult(new PollChatSessionResult(session.Id, session.Status.ToString(), session.AssignedAgentId?.ToString()));
    }
}