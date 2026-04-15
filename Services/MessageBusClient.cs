using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using UserService.DTOs.PublishDTO;

namespace UserService.Services;

public class MessageBusClient : IMessageBusClient, IDisposable
{
    private readonly IConfiguration _configuration;
    private readonly IConnection _connection;
    private readonly IChannel _channel;
    private const string ExchangeName = "trigger";

    public MessageBusClient(IConfiguration configuration)
    {
        _configuration = configuration;
        var factory = new ConnectionFactory()
        {
            HostName = _configuration["RabbitMQHost"],
            Port = int.Parse(_configuration["RabbitMQPort"] ?? "5672")
        };

        try
        {
            _connection = factory.CreateConnectionAsync().Result;
            _channel = _connection.CreateChannelAsync().Result;

            _channel.ExchangeDeclareAsync(exchange: ExchangeName, type: ExchangeType.Fanout);

            _connection.ConnectionShutdownAsync += RabbitMQ_ConnectionShutdown;

            Console.WriteLine("--> Connected to Message Bus");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"--> Could not connect to the Message Bus: {ex.Message}");
        }
    }

    private Task RabbitMQ_ConnectionShutdown(object sender, ShutdownEventArgs e)
    {
        Console.WriteLine("--> RabbitMQ Connection Shutdown");
        return Task.CompletedTask;
    }

    public void PublishNewUser(UserPublishedDto userPublishedDto)
    {
        var message = JsonSerializer.Serialize(userPublishedDto);

        if (_connection.IsOpen)
        {
            Console.WriteLine("--> RabbitMQ Connection Open, sending message...");
            SendMessage(message);
        }
        else
        {
            Console.WriteLine("--> RabbitMQ connection is closed, not sending");
        }
    }

    private void SendMessage(string message)
    {
        var body = Encoding.UTF8.GetBytes(message);

        _channel.BasicPublishAsync(exchange: ExchangeName,
                        routingKey: "",
                        body: body);
        Console.WriteLine($"--> We have sent {message}");
    }

    public void Dispose()
    {
        Console.WriteLine("MessageBus Disposed");
        if (_channel.IsOpen)
        {
            _channel.CloseAsync().Wait();
            _connection.CloseAsync().Wait();
        }
    }
}