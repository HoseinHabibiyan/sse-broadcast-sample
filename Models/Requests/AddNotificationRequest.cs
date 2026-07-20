namespace SSEBroadcastSample.Models.Requests;

public record AddNotificationRequest(string Message, DateTime ExpireDate, int[] TargetUserIds);