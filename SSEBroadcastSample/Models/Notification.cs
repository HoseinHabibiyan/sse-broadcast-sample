namespace SSEBroadcastSample.Models;

public class Notification
{
    public int Id { get; set; }
    public required string Message { get; set; }
    public DateTime CreatedDate { get; set; } = DateTime.Now;
    public DateTime ExpireDate { get; set; }
    public ICollection<NotificationTarget> NotificationTargets { get; set; }
    public ICollection<NotificationDelivery> NotificationDeliveries { get; set; }

}

public enum AudienceType
{
    AllUsers,
    SelectedUser,
    Role
} 