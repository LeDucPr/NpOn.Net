using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Common.Infrastructures.NpOn.RabbitMqExtMs;

class Program
{
    // static async Task Main(string[] args)
    // {
    //     Console.WriteLine("Starting RabbitMQ Consumer...");
    //
    //     IRabbitMqConnection rabbitMqConnection = new RabbitMqConnection("amqp://rabbitmq:password@localhost:5672/");
    //     
    //     var producer = new RabbitMqProducer(rabbitMqConnection);
    //     await producer.AddEvent(new RabbitMqEvent<TestEvent>()
    //     {
    //         MessageContent = new TestEvent
    //         {
    //             TestC = "cccccccccccccccccccccccccccccccccccccc"
    //         }
    //     });
    //     
    //     var consumer = new TestEventConsumer(rabbitMqConnection,
    //         async (message) =>
    //         {
    //             Console.WriteLine($"[Handler] Received message: {message.TestC}");
    //             await Task.Delay(100);
    //             Console.WriteLine($"[Handler] Finished processing message: {message.TestC}");
    //         });
    //
    //
    //     Console.WriteLine("Consumer is running. Press [Enter] to exit.");
    //     Console.ReadLine();
    // }
}

// [ProtoContract]
// public class TestEvent : RabbitMqMessageContent
// {
//     [ProtoMember(1)] public required string TestC { get; set; }
// }
//
// public class TestEventConsumer : RabbitMqConsumer<TestEvent>
// {
//     public TestEventConsumer(IRabbitMqConnection rabbitMqConnection, Func<TestEvent, Task> handler, bool autoAck = true)
//         : base(rabbitMqConnection, handler, autoAck)
//     {
//     }
// }

internal static class Test
{
    public static async Task TestT()
    {
        var factory = new ConnectionFactory()
        {
            Uri = new Uri("amqp://rabbitmq:password@localhost:5672/")
        };

        using var connection = await factory.CreateConnectionAsync();
        using var channel = await connection.CreateChannelAsync();

        // Setup dead-letter exchange
        await channel.ExchangeDeclareAsync(
            exchange: "dlx_exchange",
            type: "direct",
            durable: true,
            autoDelete: false,
            arguments: null);

        // Setup main queue (testQueue) with TTL and dead-lettering
        var argsM = new Dictionary<string, object>
        {
            { "x-message-ttl", 60000 }, // 60 seconds
            { "x-dead-letter-exchange", "dlx_exchange" },
            { "x-dead-letter-routing-key", "dlx_routing" }
        };

        await channel.QueueDeclareAsync(queue: "testQueue",
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: argsM);


        var body = Encoding.UTF8.GetBytes("Hello RabbitMQ!");

        BasicProperties basicProperties = new BasicProperties() { };
        await channel.BasicPublishAsync(
            exchange: "",
            routingKey: "testQueue",
            mandatory: true,
            basicProperties: basicProperties,
            body: body);

        var producerTask = Task.Run(async () =>
        {
            int counter = 1;
            while (true)
            {
                var body = Encoding.UTF8.GetBytes($"Message {counter}");
                var props = new BasicProperties { Persistent = true };

                await channel.BasicPublishAsync<BasicProperties>(
                    exchange: "",
                    routingKey: "testQueue",
                    mandatory: true,
                    basicProperties: props,
                    body: body);

                Console.WriteLine($"[Producer] Sent: Message {counter}");
                counter++;
                await Task.Delay(1000); // gửi mỗi giây
            }
        });

        var consumerTask = Task.Run(async () =>
        {
            // Setup consumers
            await SetupConsumer(channel, "testQueue", "Consumer 1");
        });

        Console.WriteLine("Consumers are running. Press [Enter] to exit.");
        Console.ReadLine(); // Keep the application running to listen for messages
    }

    private static async Task SetupConsumer(IChannel channel, string queueName, string consumerName)
    {
        var consumer = new AsyncEventingBasicConsumer(channel);
        consumer.ReceivedAsync += async (sender, ea) =>
        {
            var message = Encoding.UTF8.GetString(ea.Body.ToArray());
            Console.WriteLine($"[{consumerName}] Received from {queueName}: {message}");
            await channel.BasicAckAsync(ea.DeliveryTag, multiple: false);
        };

        await channel.BasicConsumeAsync(
            queue: queueName,
            autoAck: false,
            consumer: consumer);
    }
}