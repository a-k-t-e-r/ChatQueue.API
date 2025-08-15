using ChatQueue.Application.Chats.Commands;
using ChatQueue.Application.Chats.Handlers;
using ChatQueue.Core.Entities;
using ChatQueue.Core.Enums;
using ChatQueue.Core.Services;
using ChatQueue.Infrastructure.Policies;
using ChatQueue.Infrastructure.Queues;
using ChatQueue.Infrastructure.Repositories;
using ChatQueue.Infrastructure.Time;
using FluentAssertions;

namespace ChatQueue.xTests.Unit;

public class CreateChatSessionHandlerTests
{
    [Fact]
    public async Task WhenQueueNotFull_ShouldEnqueueAndReturnQueued()
    {
        // arrange
        var chats = new InMemoryChatRepository();
        var teams = new InMemoryTeamRepository();
        var queue = new InMemoryQueue();
        var policy = new QueuePolicy();
        var clock = new SystemClock();

        var handler = new CreateChatSessionHandler(chats, teams, queue, policy, clock);

        // act
        var res = await handler.Handle(new CreateChatSessionCommand(), CancellationToken.None);

        // assert
        res.Message.Should().Be("Queued");
        queue.Count.Should().Be(1);
        var stored = chats.Get(res.SessionId);
        stored.Should().NotBeNull();
        stored!.Status.Should().Be(ChatStatus.Queued);
    }

    [Fact]
    public async Task WhenQueueFullAndOverflowNotAllowed_ShouldRefuse()
    {
        var chats = new InMemoryChatRepository();
        var teams = new InMemoryTeamRepository(); // default
        var queue = new InMemoryQueue();
        var policy = new QueuePolicy(); // overflow allowed only during office hours
        var clock = new SystemClock();

        var primary = teams.GetPrimaryTeam(clock.UtcNow);
        var maxQueue = CapacityCalculator.MaxQueueLength(primary);
        for (int i = 0; i < maxQueue; i++)
        {
            var s = new ChatSession();
            chats.Add(s);
            queue.Enqueue(s.Id);
        }

        var handler = new CreateChatSessionHandler(chats, teams, queue, policy, clock);

        // act
        var res = await handler.Handle(new CreateChatSessionCommand(), CancellationToken.None);

        // assert
        res.Message.Should().Be("Queue full");
        res.Status.Should().Be(ChatStatus.Refused.ToString());
    }
}