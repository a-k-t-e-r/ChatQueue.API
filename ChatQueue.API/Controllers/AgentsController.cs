using ChatQueue.Application.Abstractions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace ChatQueue.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AgentsController(ILogger<ChatsController> logger,
                              ITeamRepository teams,
                              IChatRepository chats) : ControllerBase
{
    private readonly ILogger<ChatsController> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly ITeamRepository _teams = teams ?? throw new ArgumentNullException(nameof(teams));
    private readonly IChatRepository _chats = chats ?? throw new ArgumentNullException(nameof(chats));

    [HttpGet("{agentId:guid}/dashboard")]
    [EnableRateLimiting("fixed")]
    public IActionResult Dashboard(Guid agentId)
    {
        try
        {
            var agent = _teams.GetAgentById(agentId);
            if (agent is null)
                return NotFound();

            var assigned = _chats.GetByAgent(agentId)
                                 .Select(s => new { s.Id, Status = s.Status.ToString(), s.CreatedAt })
                                 .ToList();

            return Ok(new
            {
                agent.Id,
                agent.Name,
                agent.Seniority,
                agent.CurrentChats,
                Capacity = agent.EffectiveCapacity,
                AssignedSessions = assigned
            });
        }
        catch
        {
            _logger.LogError("Error while getting chat release");

            return StatusCode(500, "Internal server error");
        }
    }
}