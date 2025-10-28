using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

// RabbitMQ bağlantı ayarları
ConnectionFactory factory = new ConnectionFactory();
factory.HostName = "localhost";
factory.UserName = "admin";
factory.Password = "admin";
factory.VirtualHost = "/";

// Bağlantı oluşturma
using IConnection connection = await factory.CreateConnectionAsync();
// Kanal oluşturma
using IChannel channel = await connection.CreateChannelAsync();

Console.WriteLine("DirectExchange Consumer başlatıldı. Bağlantı kuruldu.");

// 1. Exchange oluşturma
await channel.ExchangeDeclareAsync(exchange: "directExchange", type: ExchangeType.Direct);

// 2. Kuyruk oluşturma
var queueName =  await channel.QueueDeclareAsync();

// 3. adım Bind
await channel.QueueBindAsync(
    queue: queueName.QueueName, 
    exchange: "directExchange", 
    routingKey: "directRoutingKeyQueue");

// 4. Adım 
AsyncEventingBasicConsumer consumer = new AsyncEventingBasicConsumer(channel);

await channel.BasicConsumeAsync(
    queue: queueName.QueueName, 
    autoAck: false, 
    consumer: consumer);

consumer.ReceivedAsync += async (model, ea) =>
{
    var message = Encoding.UTF8.GetString(ea.Body.Span);
    Console.WriteLine("Mesaj alındı: {0}", message);
    await channel.BasicAckAsync(ea.DeliveryTag, false);
};

// Programın kapanmaması için bekle
Console.Read();