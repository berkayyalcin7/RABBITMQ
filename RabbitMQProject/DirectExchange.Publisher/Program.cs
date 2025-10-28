using RabbitMQ.Client;
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

Console.WriteLine("DirectExchange Publisher başlatıldı. Bağlantı kuruldu.");

// Exchange oluşturma
await channel.ExchangeDeclareAsync(exchange: "directExchange", type: ExchangeType.Direct);

while(true)
{
    Console.Write("Mesaj : ");
    string message = Console.ReadLine() ?? string.Empty;
    var messageBytes = Encoding.UTF8.GetBytes(message);
    // Mesajı kuyruğa atar.
    await channel.BasicPublishAsync(
        exchange: "directExchange", 
        routingKey: "directRoutingKeyQueue", 
        body: messageBytes);
}

// Programın kapanmaması için bekle
Console.Read();