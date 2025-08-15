using ChatQueue.Application.Chats.Commands;
using ChatQueue.Application.Chats.Handlers;
using ChatQueue.Core.Entities;
using ChatQueue.Infrastructure.Repositories;
using ChatQueue.Infrastructure.Time;
using FluentAssertions;

namespace ChatQueue.xTests.Unit;

public class PollChatSessionHandlerTests
{
    [Fact]
    public async Task PollUpdatesLastPolledAtAndReturnsStatus()
    {
        // arrange
        var chats = new InMemoryChatRepository();
        var clock = new SystemClock();
        var handler = new PollChatSessionHandler(chats, clock);

        var session = new ChatSession();
        chats.Add(session);

        // act
        var res = await handler.Handle(new PollChatSessionCommand(session.Id), CancellationToken.None);

        // assert
        res.Status.Should().Be(session.Status.ToString());
        var stored = chats.Get(session.Id);
        stored.Should().NotBeNull();
        stored!.LastPolledAt.Should().Be(session.LastPolledAt);
    }

    [Fact]
    public async Task PollNonexistent_Throws()
    {
        var chats = new InMemoryChatRepository();
        var clock = new SystemClock();
        var handler = new PollChatSessionHandler(chats, clock);

        await Assert.ThrowsAsync<KeyNotFoundException>(() => handler.Handle(new PollChatSessionCommand(Guid.NewGuid()), CancellationToken.None));
    }
}