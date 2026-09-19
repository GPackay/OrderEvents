using Contracts;
using System.Text.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

var factory = new ConnectionFactory
{
    HostName = "localhost"
};

await using var connection = await factory.CreateConnectionAsync();
await using var channel = await connection.CreateChannelAsync();

await channel.QueueDeclareAsync(
    queue: "order.placed",
    durable: true,
    exclusive: false,
    autoDelete: false,
    arguments: null);

await channel.BasicQosAsync(
    prefetchSize: 0,
    prefetchCount: 10,
    global: false);

var consumer = new AsyncEventingBasicConsumer(channel);

consumer.ReceivedAsync += async (sender, ea) =>
{
    try
    {
        var order = JsonSerializer.Deserialize<OrderPlaced>(
            ea.Body.Span);

        if (order is null)
        {
            throw new Exception("Message could not be deserialized.");
        }

        Console.WriteLine(
            $"Processing Order: {order.OrderId} | " +
            $"Student: {order.StudentId} | " +
            $"Total: {order.Total:C}");

        // Simulate processing
        await Task.Delay(1000);

        await channel.BasicAckAsync(
            deliveryTag: ea.DeliveryTag,
            multiple: false);

        Console.WriteLine(
            $"Acknowledged Order: {order.OrderId}");
    }
    catch (Exception ex)
    {
        Console.WriteLine(
            $"Failed processing message: {ex.Message}");

        await channel.BasicNackAsync(
            deliveryTag: ea.DeliveryTag,
            multiple: false,
            requeue: false);
    }
};

await channel.BasicConsumeAsync(
    queue: "order.placed",
    autoAck: false,
    consumer: consumer);

Console.WriteLine("Consumer running...");
Console.WriteLine("Press Enter to exit.");

Console.ReadLine();