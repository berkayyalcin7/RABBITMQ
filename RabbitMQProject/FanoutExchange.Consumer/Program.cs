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

Console.WriteLine("FanoutExchange Consumer başlatıldı. Bağlantı kuruldu.");

await channel.ExchangeDeclareAsync(exchange:"fanoutExchangeExample", type: ExchangeType.Fanout);

Console.WriteLine("Kuyruk adını giriniz : ");

string _queueName = Console.ReadLine();

// Kuyruk oluşturma
await channel.QueueDeclareAsync(
    queue: _queueName,
    exclusive: false,
    autoDelete: false);

// Kuyruk bağlama
await channel.QueueBindAsync(
    _queueName,
    exchange: "fanoutExchangeExample",
    routingKey: string.Empty);

AsyncEventingBasicConsumer consumer = new AsyncEventingBasicConsumer(channel);

await channel.BasicConsumeAsync(
    queue: _queueName, 
    autoAck: true, 
    consumer);

consumer.ReceivedAsync += async (sender, e) =>
{
    string message = Encoding.UTF8.GetString(e.Body.Span);

    Console.WriteLine($"Gelen Mesaj: {message}");
};

// Programın kapanmaması için bekle
Console.Read();