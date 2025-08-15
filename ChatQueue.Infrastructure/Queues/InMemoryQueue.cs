using ChatQueue.Application.Abstractions;

namespace ChatQueue.Infrastructure.Queues;

public sealed class InMemoryQueue : IQueueRepository
{
    private readonly Queue<Guid> _queue = new();
    private readonly HashSet<Guid> _set = new();

    public int Count => _queue.Count;

    public void Enqueue(Guid chatId)
    {
        if (_set.Add(chatId))
            _queue.Enqueue(chatId);
    }

    public Guid? Dequeue()
    {
        if (_queue.Count == 0)
            return null;

        var id = _queue.Dequeue();
        _set.Remove(id);

        return id;
    }

    public bool Contains(Guid id) => _set.Contains(id);

    public IEnumerable<Guid> Snapshot() => _queue.ToArray();
}