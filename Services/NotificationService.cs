using Microsoft.EntityFrameworkCore;
using SSEBroadcastSample.Data;
using SSEBroadcastSample.Models.Entities;
using SSEBroadcastSample.Models.Requests;

namespace SSEBroadcastSample.Services;

public class NotificationService(AppDbContext db, NotificationHub hub)
{
    public async Task AddNotificationAsync(AddNotificationRequest request, CancellationToken ct)
    {
        List<NotificationTarget> targets = new();
        if (request.TargetUserIds.Any())
        {
            targets = request.TargetUserIds.Select(userId => new NotificationTarget
            {
                TargetType = NotificationTargetType.User,
                TargetId = userId,
            }).ToList();
        }
        else
        {
            targets.Add(new NotificationTarget
            {
                TargetType = NotificationTargetType.All,
            });
        }

        Notification notification = new()
        {
            Message = request.Message,
            ExpireDate = request.ExpireDate,
            NotificationTargets = targets
        };

        db.Notifications.Add(notification);
        await db.SaveChangesAsync(ct);

        if (request.TargetUserIds.Any())
        {
            foreach (var userId in request.TargetUserIds)
                await hub.Publish(userId, notification);
        }
        else
        {
            await hub.Publish(notification);
        }
    }

    public async Task<List<Notification>> GetPending(int userId, CancellationToken ct)
    {
        return await db.Notifications
            .AsNoTracking()
            .Where(n =>
                n.ExpireDate > DateTime.UtcNow &&
                n.NotificationTargets.Any(t =>
                    (t.TargetType == NotificationTargetType.User && t.TargetId == userId) || t.TargetType == NotificationTargetType.All) &&
                n.NotificationDeliveries.All(d => d.UserId != userId))
            .ToListAsync(ct);
    }
    
    public async Task AddDelivery(NotificationDelivery delivery, CancellationToken ct)
    {
        db.NotificationDeliveries.Add(delivery);
        await db.SaveChangesAsync(ct);
    }

    public async Task AddDeliveries(List<NotificationDelivery> deliveries, CancellationToken ct)
    {
        db.NotificationDeliveries.AddRange(deliveries);
        await db.SaveChangesAsync(ct);
    }
}