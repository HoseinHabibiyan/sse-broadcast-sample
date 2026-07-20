namespace SSEBroadcastSample.Models.Entities;

public class NotificationDelivery
{
 public int Id { get; set; }
 public int NotificationId { get; set; }
 public Notification Notification { get; set; }
 public int UserId { get; set; }
 public User User { get; set; }
}