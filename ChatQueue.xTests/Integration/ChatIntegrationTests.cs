using ChatQueue.Application.Chats.Commands;
using ChatQueue.Core.Enums;
using FluentAssertions;
using System.Net;
using System.Net.Http.Json;

namespace ChatQueue.xTests.Integration;

public class ChatIntegrationTests(CustomWebApplicationFactory factory) : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory = factory;

    [Fact]
    public async Task CreateChat_Then_Poll_ReturnsQueuedThenPolled()
    {
        using var client = _factory.CreateClient();

        // create chat
        var createResp = await client.PostAsync("/api/chats", null);
        createResp.StatusCode.Should().Be(HttpStatusCode.OK);
        var createBody = await createResp.Content.ReadFromJsonAsync<CreateChatSessionResult>();
        createBody.Should().NotBeNull();
        createBody!.Status.Should().Be(ChatStatus.Queued.ToString());

        // poll the chat
        var pollResp = await client.PostAsync($"/api/chats/{createBody.SessionId}/poll", null);
        pollResp.StatusCode.Should().Be(HttpStatusCode.OK);

        var pollBody = await pollResp.Content.ReadFromJsonAsync<PollChatSessionResult>();
        pollBody.Should().NotBeNull();
        pollBody!.Status.Should().Be(ChatStatus.Queued.ToString());
    }

    [Fact]
    public async Task QueueMonitor_MarksInactive_WhenNoPollsFor3Sec()
    {
        using var client = _factory.CreateClient();

        // Create a chat
        var createResp = await client.PostAsync("/api/chats", null);
        createResp.StatusCode.Should().Be(HttpStatusCode.OK);
        var createBody = await createResp.Content.ReadFromJsonAsync<CreateChatSessionResult>();
        createBody.Should().NotBeNull();

        var id = createBody!.SessionId;

        // Ensure initial status is queued
        var pollResp1 = await client.PostAsync($"/api/chats/{id}/poll", null);
        pollResp1.StatusCode.Should().Be(HttpStatusCode.OK);
        var pollBody1 = await pollResp1.Content.ReadFromJsonAsync<PollChatSessionResult>();
        pollBody1!.Status.Should().Be(ChatStatus.Queued.ToString());

        // Advance the test clock by 4 seconds to exceed the 3 second threshold
        _factory.Clock.Advance(TimeSpan.FromSeconds(4));

        // wait a little to allow hosted service loop to run once (it loops every ~1s in the implementation)
        await Task.Delay(1200);

        // Now poll again - the hosted service should have marked it as inactive
        var pollResp2 = await client.PostAsync($"/api/chats/{id}/poll", null);
        pollResp2.StatusCode.Should().Be(HttpStatusCode.OK);
        var pollBody2 = await pollResp2.Content.ReadFromJsonAsync<PollChatSessionResult>();
        // Because the session was previously marked Inactive, result.Status should reflect that.
        pollBody2!.Status.Should().Be(ChatStatus.Inactive.ToString());
    }
}