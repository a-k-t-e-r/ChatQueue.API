using ChatQueue.Application.Abstractions;
using ChatQueue.Application.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Text.Json;

namespace ChatQueue.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AgentsController(ILogger<ChatsController> logger,
                              ICacheService cache,
                              ITeamRepository teams,
                              IChatRepository chats) : ControllerBase
{
    private readonly ILogger<ChatsController> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly ICacheService _cache = cache ?? throw new ArgumentNullException(nameof(cache));
    private readonly ITeamRepository _teams = teams ?? throw new ArgumentNullException(nameof(teams));
    private readonly IChatRepository _chats = chats ?? throw new ArgumentNullException(nameof(chats));

    [HttpGet("{agentId:guid}/dashboard")]
    [EnableRateLimiting("fixed")]
    public async Task<IActionResult> DashboardAsync(Guid agentId)
    {
        try
        {
            string cacheKey = $"agent_dashboard_{agentId}";
            var cachedDashboard = await _cache.GetAsync<string>(cacheKey);
            if (cachedDashboard != null)
            {
                _logger.LogDebug($"Retrieved dashboard for agent {agentId} from cache");
                return Ok(JsonSerializer.Deserialize<object>(cachedDashboard));
            }

            var agent = _teams.GetAgentById(agentId);
            if (agent is null)
                return NotFound();

            var assigned = _chats.GetByAgent(agentId)
                                 .Select(s => new { s.Id, Status = s.Status.ToString(), s.CreatedAt })
                                 .ToList();

            var response = new
            {
                agent.Id,
                agent.Name,
                agent.Seniority,
                agent.CurrentChats,
                Capacity = agent.EffectiveCapacity,
                AssignedSessions = assigned
            };

            await _cache.SetAsync(cacheKey, JsonSerializer.Serialize(response), TimeSpan.FromMinutes(5));

            return Ok(response);
        }
        catch
        {
            _logger.LogError("Error while getting chat release");

            throw;
        }
    }
}