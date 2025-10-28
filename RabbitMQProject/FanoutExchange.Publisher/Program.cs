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

Console.WriteLine("FanoutExchange Publisher başlatıldı. Bağlantı kuruldu.");

// Bind olmuş bütün kuyruklar için mesaj gönderimi yapılır
await channel.ExchangeDeclareAsync(
    exchange:"fanoutExchangeExample",
    type: ExchangeType.Fanout);

for (int i = 1; i <= 10; i++)
{
    await Task.Delay(200);

    byte[] message = Encoding.UTF8.GetBytes($"Merhaba {i}");

    await channel.BasicPublishAsync(
        exchange: "fanoutExchangeExample",
        routingKey: string.Empty,
        body: message);
}






    // Programın kapanmaması için bekle
    Console.Read();