using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using UserService.Data;
using UserService.DTOs.PublishDTO;

namespace UserService.AsyncDataServices;

public class OutboxPublisher : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IMessageBusClient _messageBusClient;

    public OutboxPublisher(IServiceScopeFactory scopeFactory, IMessageBusClient messageBusClient)
    {
        _scopeFactory = scopeFactory;
        _messageBusClient = messageBusClient;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Console.WriteLine("--> Outbox Publisher Service Started");

        while (!stoppingToken.IsCancellationRequested)
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<UserDbContext>();

                var messages = await dbContext.OutboxMessages
                    .Where(m => m.ProcessedOn == null)
                    .OrderBy(m => m.OccuredOn)
                    .Take(10)
                    .ToListAsync(stoppingToken);

                foreach (var message in messages)
                {
                    try
                    {
                        Console.WriteLine($"--> Processing Outbox Message: {message.Id}");

                        if (message.Type == "User_Published")
                        {
                            var userPublishedDto = JsonSerializer.Deserialize<UserPublishedDto>(message.Content);
                            if (userPublishedDto != null)
                            {
                                await _messageBusClient.PublishNewUser(userPublishedDto);
                            }
                        }

                        message.ProcessedOn = DateTime.UtcNow;
                        await dbContext.SaveChangesAsync(stoppingToken);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"--> Could not process outbox message {message.Id}: {ex.Message}");
                    }
                }
            }

            await Task.Delay(10000, stoppingToken); // Poll every 10 seconds
        }
    }
}