using ChatQueue.Application.Abstractions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace ChatQueue.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AgentsController(ITeamRepository teams, IChatRepository chats) : ControllerBase
{
    private readonly ITeamRepository _teams = teams;
    private readonly IChatRepository _chats = chats;

    [HttpGet("{agentId:guid}/dashboard")]
    [EnableRateLimiting("fixed")]
    public IActionResult Dashboard(Guid agentId)
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
}