using ChatQueue.Application.Abstractions;
using ChatQueue.Core.Enums;
using ChatQueue.Core.Services;
using Microsoft.Extensions.Hosting;

namespace ChatQueue.Infrastructure.HostedServices;

public sealed class AssignmentHostedService(IQueueRepository queue,
                                            IChatRepository chats,
                                            ITeamRepository teams,
                                            IQueuePolicy policy,
                                            IDateTimeProvider clock) : BackgroundService
{
    private readonly IQueueRepository _queue = queue;
    private readonly IChatRepository _chats = chats;
    private readonly ITeamRepository _teams = teams;
    private readonly IQueuePolicy _policy = policy;
    private readonly IDateTimeProvider _clock = clock;

    private int _rrIndex = 0;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var now = _clock.UtcNow;
            var teams = _teams.GetActiveTeams(now).ToList();
            var primary = teams.First();
            var candidates = teams.SelectMany(t => t.Agents.Select(a => (team: t, agent: a)))
                                  .OrderBy(t => t.agent.Seniority) // jr first
                                  .ToList();

            if (_policy.OverflowAllowed(now))
            {
                // if queue >= primary max queue, bring overflow agents into candidates
                var maxQueue = CapacityCalculator.MaxQueueLength(primary);
                if (_queue.Count >= maxQueue)
                {
                    var overflowTeam = _teams.GetOverflowTeam();
                    candidates.AddRange(overflowTeam.Agents.Select(a => (team: overflowTeam, agent: a)));
                }
            }

            if (_queue.Count > 0 && candidates.Count != 0)
            {
                var chatId = _queue.Dequeue();
                if (chatId is not null)
                {
                    var session = _chats.Get(chatId.Value);
                    if (session is not null && session.Status == ChatStatus.Queued)
                    {
                        // Round robin over candidates, but skip those at capacity
                        for (int i = 0; i < candidates.Count; i++)
                        {
                            var idx = (_rrIndex + i) % candidates.Count;
                            var c = candidates[idx];
                            if (c.agent.CanAcceptMore())
                            {
                                c.agent.AssignChat();
                                session.Assign(c.agent.Id);
                                _chats.Update(session);
                                _rrIndex = idx + 1;
                                break;
                            }
                        }
                    }
                }
            }

            await Task.Delay(200, stoppingToken);
        }
    }
}