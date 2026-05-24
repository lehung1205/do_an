using System.Collections.Concurrent;
using JobPortal.API.Services.Interface;

namespace JobPortal.API.Services.Implementation;

public class ChatPresenceService : IChatPresenceService
{
    private readonly ConcurrentDictionary<string, long> _connections = new();
    private readonly ConcurrentDictionary<long, int> _userConnectionCounts = new();

    public void RegisterConnection(string connectionId, long userId)
    {
        if (_connections.TryAdd(connectionId, userId))
        {
            _userConnectionCounts.AddOrUpdate(userId, 1, static (_, count) => count + 1);
        }
    }

    public long? RemoveConnection(string connectionId)
    {
        if (!_connections.TryRemove(connectionId, out var userId))
        {
            return null;
        }

        _userConnectionCounts.AddOrUpdate(userId, 0, static (_, count) => Math.Max(0, count - 1));
        if (_userConnectionCounts.TryGetValue(userId, out var remaining) && remaining == 0)
        {
            _userConnectionCounts.TryRemove(userId, out _);
        }

        return userId;
    }

    public bool IsUserOnline(long userId) =>
        _userConnectionCounts.TryGetValue(userId, out var count) && count > 0;
}
