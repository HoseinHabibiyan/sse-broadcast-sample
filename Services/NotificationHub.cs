using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Threading.Channels;
using SSEBroadcastSample.Models;
using SSEBroadcastSample.Models.Entities;

namespace SSEBroadcastSample.Services;

public class NotificationHub
{
    private readonly ConcurrentDictionary<int, Channel<Notification>> _connections = new();

    public async Task Publish(int userId, Notification notification)
    {
        if (_connections.TryGetValue(userId, out var channel))
        {
            try
            {
                await channel.Writer.WriteAsync(notification);
            }
            catch (ChannelClosedException)
            {
                Disconnect(userId);
            }
        }
    }
    
    public async Task Publish(Notification notification)
    {
        foreach (var connection in _connections)
        {
            try
            {
                await connection.Value.Writer.WriteAsync(notification);
            }
            catch (ChannelClosedException)
            {
                Disconnect(connection.Key);
            }
        }
    }
    
    public async IAsyncEnumerable<Notification> SubscribeAsync(int userId, [EnumeratorCancellation] CancellationToken ct)
    {
        var channel = Connect(userId);

        try
        {
            await foreach (var notification in channel.ReadAllAsync(ct))
            {
                yield return notification;
            }
        }
        finally
        {
            Disconnect(userId);
        }
    }

    private ChannelReader<Notification> Connect(int userId)
    {
        var channel = Channel.CreateUnbounded<Notification>();

        _connections[userId] = channel;

        return channel.Reader;
    }
    
    private void Disconnect(int userId)
    {
        if (_connections.TryRemove(userId, out var channel))
        {
            channel.Writer.TryComplete();
        }
    }
}