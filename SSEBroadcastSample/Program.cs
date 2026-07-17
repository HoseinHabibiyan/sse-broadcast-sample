using System.Collections.Concurrent;
using Microsoft.EntityFrameworkCore;
using SSEBroadcastSample.Context;
using SSEBroadcastSample.Models;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyHeader()
            .AllowAnyMethod()
            .AllowAnyOrigin();
    });
});

using (var scope = builder.Services.BuildServiceProvider().CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
}

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseDefaultFiles();
app.UseCors();

app.UseHttpsRedirection();

app.MapScalarApiReference();

ConcurrentDictionary<int, StreamWriter> connections = new();

app.MapPost("/notification", async (AddNotifyRequest request,AppDbContext db,CancellationToken ct) =>
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

    return Results.Ok();
});

app.MapGet("/notifications/event/{userId:int}",
    async (
        int userId,
        AppDbContext db,
        HttpContext context,
        CancellationToken ct) =>
    {
        context.Response.Headers.ContentType = "text/event-stream";
        context.Response.Headers.CacheControl = "no-cache";

        await context.Response.Body.FlushAsync(ct);

        var writer = new StreamWriter(context.Response.Body);

        connections[userId] = writer;
        
        while (!ct.IsCancellationRequested)
        {
            var pending = await db.NotificationTargets
                .Include(x => x.Notification)
                .ThenInclude(x => x.NotificationDeliveries)
                .Where(x =>
                    ((x.TargetType == NotificationTargetType.User && x.TargetId == userId) ||
                    x.TargetType == NotificationTargetType.All) &&
                    x.Notification.NotificationDeliveries.All(d => d.UserId != userId)
                    && x.Notification.ExpireDate > DateTime.UtcNow)
                .ToListAsync(ct);

            if (pending.Any())
            {
                foreach (var item in pending)
                {
                    await writer.WriteAsync(
                        $"data: {item.Notification.Message}\n\n");

                    await writer.FlushAsync();

                    db.NotificationDeliveries.Add(new NotificationDelivery()
                    {
                        UserId = userId,
                        NotificationId = item.NotificationId
                    });
                }

                await db.SaveChangesAsync(ct);
            }

            await Task.Delay(5000, ct);
        }
    });

app.MapGet("/users", async (AppDbContext db,CancellationToken ct) =>
{
    var users = await db.Users.Select(u => new { u.Id, u.UserName }).ToListAsync(ct);
    return Results.Ok(users);
});

app.Run();

record AddNotifyRequest(string Message, DateTime ExpireDate, int[] TargetUserIds);