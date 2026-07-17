# Server Sent Events Notification Service Sample

## Run Project

To start the project, execute:
```bash
start-dev.bat
```

Alternatively, you can run the project manually:
```bash
cd wwwroot
npx http-server
```

To start server from the project root:
```bash
dotnet run
```

## Create Notification

Use the Scalar API documentation to send a new notification.

Endpoint:
POST http://localhost:5171/notification

Example:

```json
{
  "message": "System maintenance tonight",
  "expireDate": "2026-12-31",
  "targetUserIds": []
}
```
> Add user IDs to `targetUserIds` for targeted notifications, or set it to `[]` to publish the notification to all users.

## Receive Notifications

Open the client page to receive real-time notifications:
http://127.0.0.1:8080/index.html
