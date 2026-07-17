# Server Sent Events Notification Service Sample

## Run Project

To start the project, simply run:

http://localhost:5171/scalar


---

## Create Notification

Use the notification endpoint:

http://localhost:5171/notification

Example:

```json
{
  "message": "System maintenance tonight",
  "expireDate": "2026-12-31",
  "targetUserIds": []
}
```
> Add user IDs to `targetUserIds` for targeted notifications, or set it to `[]` to publish the notification to all users.
