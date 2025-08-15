using ChatQueue.Application.Abstractions;
using ChatQueue.Application.Chats.Commands;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ChatQueue.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ChatsController(IMediator mediator, IChatRepository chats, ITeamRepository teams) : ControllerBase
{
    private readonly IMediator _mediator = mediator;
    private readonly IChatRepository _chats = chats;
    private readonly ITeamRepository _teams = teams;

    [HttpPost]
    public async Task<IActionResult> CreateChat()
    {
        var result = await _mediator.Send(new CreateChatSessionCommand());

        return Ok(result);
    }

    [HttpPost("{id:guid}/poll")]
    public async Task<IActionResult> PollChat(Guid id)
    {
        var result = await _mediator.Send(new PollChatSessionCommand(id));

        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public IActionResult GetChatStatus(Guid id)
    {
        var chatStatus = _chats.Get(id);
        if (chatStatus is null)
            return NotFound();

        return Ok(new
        {
            chatStatus.Id,
            Status = chatStatus.Status.ToString(),
            AssignedAgentId = chatStatus.AssignedAgentId?.ToString(),
            chatStatus.CreatedAt,
            chatStatus.LastPolledAt
        });
    }

    [HttpPost("{id:guid}/release")]
    public IActionResult ReleaseChat(Guid id)
    {
        var chatRelease = _chats.Get(id);
        if (chatRelease is null)
            return NotFound();
        if (chatRelease.AssignedAgentId is null)
            return BadRequest(new { message = "No agent assigned" });

        var agent = _teams.GetAgentById(chatRelease.AssignedAgentId.Value);
        agent?.ReleaseChat();

        chatRelease.MarkRefused();
        _chats.Update(chatRelease);

        return Ok(new { chatRelease.Id, chatRelease.Status });
    }

    [HttpPost("{id:guid}/complete")]
    public IActionResult CompleteChat(Guid id)
    {
        var s = _chats.Get(id);
        if (s is null) return NotFound();
        if (s.AssignedAgentId is null) return BadRequest(new { message = "No agent assigned" });

        var agent = _teams.GetAgentById(s.AssignedAgentId.Value);
        agent?.ReleaseChat();

        s.MarkCompleted();
        _chats.Update(s);

        return Ok(new { s.Id, s.Status });
    }
}