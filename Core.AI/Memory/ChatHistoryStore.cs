using System.Collections.Concurrent;

namespace Core.AI.Memory;

public class ChatHistoryStore
{
    private readonly ConcurrentDictionary<string, List<(string Role, string Content)>> _histories = new();

    public void AddMessage(string userId, string role, string content)
    {
        var history = _histories.GetOrAdd(userId, _ => new List<(string, string)>());
        history.Add((role, content));
    }

    public List<(string Role, string Content)> GetHistory(string userId)
    {
        return _histories.TryGetValue(userId, out var history) ? history : new List<(string, string)>();
    }

    public void Clear(string userId)
    {
        _histories.TryRemove(userId, out _);
    }
}
