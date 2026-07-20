using System.Net.ServerSentEvents;
using System.Runtime.CompilerServices;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using SSEBroadcastSample.Data;
using SSEBroadcastSample.Models.Entities;
using SSEBroadcastSample.Models.Requests;
using SSEBroadcastSample.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddMemoryCache();

builder.Services.AddSingleton<NotificationHub>();
builder.Services.AddScoped<NotificationService>();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyHeader()
            .AllowAnyMethod()
            .AllowAnyOrigin();
    });
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseDefaultFiles();
app.UseCors();

app.UseHttpsRedirection();

app.MapScalarApiReference();

app.MapGet("/notifications/event/{userId:int}", (
    int userId,
    NotificationService notificationService,
    NotificationHub hub,
    CancellationToken ct) => Results.ServerSentEvents(
    StreamNotifications(userId, notificationService, hub, ct)));

async IAsyncEnumerable<SseItem<string>> StreamNotifications(int userId,
    NotificationService notificationService,
    NotificationHub hub,
    [EnumeratorCancellation] CancellationToken ct)
{
    var pendingNotifications = await notificationService.GetPending(userId, ct);

    List<NotificationDelivery> deliveries = new();

    foreach (var notification in pendingNotifications)
    {
        yield return new SseItem<string>(notification.Message);

        deliveries.Add(new NotificationDelivery()
        {
            UserId = userId,
            NotificationId = notification.Id
        });
    }

    if (deliveries.Any())
        await notificationService.AddDelivery(deliveries, ct);

    await foreach (var notification in hub.SubscribeAsync(userId, ct))
    {
        yield return new SseItem<string>(notification.Message);

        await notificationService.AddDelivery([
            new NotificationDelivery
            {
                UserId = userId,
                NotificationId = notification.Id
            }
        ], ct);
    }
}

app.MapPost("/notification", async (AddNotificationRequest request, NotificationService notificationService, CancellationToken ct) =>
{
    await notificationService.AddNotificationAsync(request, ct);

    return Results.Ok();
});

app.MapGet("/users", async (AppDbContext db, CancellationToken ct) =>
{
    var users = await db.Users.AsNoTracking()
        .Select(u => new { u.Id, u.UserName }).ToListAsync(ct);
    return Results.Ok(users);
});

app.Run();