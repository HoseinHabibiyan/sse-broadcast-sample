namespace SSEBroadcastSample.Models;

public class NotificationTarget
{
    public int Id { get; set; }
    public int NotificationId { get; set; }
    public NotificationTargetType TargetType { get; set; }
    public int? TargetId { get; set; }
    public Notification Notification { get; set; }
}

public enum NotificationTargetType
{
    All,
    User
}