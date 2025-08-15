using ChatQueue.Application.Abstractions;
using ChatQueue.Application.Chats.Commands;
using ChatQueue.Core.Entities;
using ChatQueue.Core.Services;
using MediatR;

namespace ChatQueue.Application.Chats.Handlers;

public sealed class CreateChatSessionHandler(IChatRepository chats,
                                             ITeamRepository teams,
                                             IQueueRepository queue,
                                             IQueuePolicy policy,
                                             IDateTimeProvider clock) : IRequestHandler<CreateChatSessionCommand, CreateChatSessionResult>
{
    private readonly IChatRepository _chats = chats ?? throw new ArgumentNullException(nameof(chats));
    private readonly ITeamRepository _teams = teams ?? throw new ArgumentNullException(nameof(teams));
    private readonly IQueueRepository _queue = queue ?? throw new ArgumentNullException(nameof(queue));
    private readonly IQueuePolicy _policy = policy ?? throw new ArgumentNullException(nameof(policy));
    private readonly IDateTimeProvider _clock = clock ?? throw new ArgumentNullException(nameof(clock));

    public Task<CreateChatSessionResult> Handle(CreateChatSessionCommand request, CancellationToken cancellationToken)
    {
        var utcTimeNow = _clock.UtcNow;
        var primary = _teams.GetPrimaryTeam(utcTimeNow);
        var capacity = CapacityCalculator.TeamCapacity(primary);
        var maxQueue = CapacityCalculator.MaxQueueLength(primary);
        var allowOverflow = _policy.OverflowAllowed(utcTimeNow);

        if (_queue.Count >= maxQueue && !allowOverflow)
        {
            var refused = new ChatSession();
            refused.MarkRefused();

            _chats.Add(refused);

            return Task.FromResult(new CreateChatSessionResult(refused.Id, refused.Status.ToString(), "Queue full"));
        }

        // if queue is full but overflow is allowed, we still enqueue
        var session = new ChatSession();
        _chats.Add(session);
        _queue.Enqueue(session.Id);

        return Task.FromResult(new CreateChatSessionResult(session.Id, session.Status.ToString(), "Queued"));
    }
}