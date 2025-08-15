using ChatQueue.Application.Abstractions;
using ChatQueue.Application.Chats.Commands;
using ChatQueue.Application.Services;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Text.Json;

namespace ChatQueue.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ChatsController(IMediator mediator,
                             ILogger<ChatsController> logger,
                             ICacheService cache,
                             IChatRepository chats,
                             ITeamRepository teams) : ControllerBase
{
    private readonly IMediator _mediator = mediator;
    private readonly ILogger<ChatsController> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly ICacheService _cache = cache ?? throw new ArgumentNullException(nameof(cache));
    private readonly IChatRepository _chats = chats ?? throw new ArgumentNullException(nameof(chats));
    private readonly ITeamRepository _teams = teams ?? throw new ArgumentNullException(nameof(teams));

    [HttpPost]
    [EnableRateLimiting("fixed")]
    public async Task<IActionResult> CreateChat()
    {
        try
        {
            var result = await _mediator.Send(new CreateChatSessionCommand());
            _logger.LogDebug(result.Message);

            return Ok(result);
        }
        catch
        {
            _logger.LogError("Error while creating chat");

            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPost("{id:guid}/poll")]
    [EnableRateLimiting("fixed")]
    public async Task<IActionResult> PollChat(Guid id)
    {
        try
        {
            var result = await _mediator.Send(new PollChatSessionCommand(id));
            _logger.LogDebug(result.Status);

            return Ok(result);
        }
        catch
        {
            _logger.LogError("Error while polling chat");

            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("{id:guid}")]
    [EnableRateLimiting("fixed")]
    public async Task<IActionResult> GetChatStatusAsync(Guid id)
    {
        try
        {
            string cacheKey = $"chat_status_{id}";
            var cachedStatus = await _cache.GetAsync<string>(cacheKey);
            if (cachedStatus != null)
            {
                _logger.LogDebug($"Retrieved chat status {id} from cache");

                return Ok(JsonSerializer.Deserialize<object>(cachedStatus));
            }

            var chatStatus = _chats.Get(id);
            if (chatStatus is null)
                return NotFound();

            _logger.LogDebug(chatStatus.Status.ToString());

            var response = new
            {
                chatStatus.Id,
                Status = chatStatus.Status.ToString(),
                AssignedAgentId = chatStatus.AssignedAgentId?.ToString(),
                chatStatus.CreatedAt,
                chatStatus.LastPolledAt
            };

            await _cache.SetAsync(cacheKey, JsonSerializer.Serialize(response), TimeSpan.FromMinutes(5));

            return Ok(response);
        }
        catch
        {
            _logger.LogError("Error while getting chat status");

            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPost("{id:guid}/release")]
    [EnableRateLimiting("fixed")]
    public async Task<IActionResult> ReleaseChatAsync(Guid id)
    {
        try
        {
            var chatRelease = _chats.Get(id);
            if (chatRelease is null)
                return NotFound();
            if (chatRelease.AssignedAgentId is null)
                return BadRequest(new { message = "No agent assigned" });

            var agent = _teams.GetAgentById(chatRelease.AssignedAgentId.Value);
            agent?.ReleaseChat();
            _logger.LogDebug(agent.Name);

            chatRelease.MarkRefused();
            _chats.Update(chatRelease);

            // Invalidate cache
            await _cache.RemoveAsync($"chat_status_{id}");

            return Ok(new { chatRelease.Id, chatRelease.Status });
        }
        catch
        {
            _logger.LogError("Error while getting chat release");

            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPost("{id:guid}/complete")]
    [EnableRateLimiting("fixed")]
    public async Task<IActionResult> CompleteChatAsync(Guid id)
    {
        try
        {
            var s = _chats.Get(id);
            if (s is null)
                return NotFound();
            if (s.AssignedAgentId is null)
                return BadRequest(new { message = "No agent assigned" });

            var agent = _teams.GetAgentById(s.AssignedAgentId.Value);
            agent?.ReleaseChat();

            s.MarkCompleted();
            _chats.Update(s);

            // Invalidate cache
            await _cache.RemoveAsync($"chat_status_{id}");

            return Ok(new { s.Id, s.Status });
        }
        catch
        {
            _logger.LogError("Error while getting chat complete");

            return StatusCode(500, "Internal server error");
        }
    }
}