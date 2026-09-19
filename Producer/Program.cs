using Contracts;
using RabbitMQ.Client;
using System.Text.Json;

var factory = new ConnectionFactory
{
    HostName = "localhost"
};

await using var connection =
    await factory.CreateConnectionAsync();

await using var channel =
    await connection.CreateChannelAsync();

await channel.QueueDeclareAsync(
    queue: "order.placed",
    durable: true,
    exclusive: false,
    autoDelete: false,
    arguments: null);

var order = new OrderPlaced(
    Guid.NewGuid(),
    "2026-00123",
    149.75m,
    DateTime.UtcNow);

var body = JsonSerializer.SerializeToUtf8Bytes(order);

var props = new BasicProperties
{
    Persistent = true,
    ContentType = "application/json",
    MessageId = order.OrderId.ToString(),
    Timestamp = new AmqpTimestamp(
        DateTimeOffset.UtcNow.ToUnixTimeSeconds())
};

await channel.BasicPublishAsync(
    exchange: string.Empty,
    routingKey: "order.placed",
    mandatory: true,
    basicProperties: props,
    body: body);

Console.WriteLine(
    $"Published OrderPlaced {order.OrderId}");