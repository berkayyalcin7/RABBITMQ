using RabbitMQ.Client;
using System.Text;

// RabbitMQ bağlantı ayarları
ConnectionFactory factory = new ConnectionFactory();
factory.HostName = "localhost";
factory.UserName = "admin";
factory.Password = "admin123";
factory.VirtualHost = "/";

// Bağlantı oluşturma
using IConnection connection = await factory.CreateConnectionAsync();
// Kanal oluşturma
using IChannel channel = await connection.CreateChannelAsync();

Console.WriteLine("HeaderExchange Publisher başlatıldı. Bağlantı kuruldu.");

// Exchange oluşturma
await channel.ExchangeDeclareAsync(
    exchange: "headerExchange",
    type: ExchangeType.Headers
);

for (int i = 0; i < 100; i++)
{
    await Task.Delay(200);

    byte[] message = Encoding.UTF8.GetBytes($"Merhaba {i}");

    Console.WriteLine("Mesajın gönderileceği Header bilgilerini belirtiniz (örn: priority:high,type:notification)");
    string headerInput = Console.ReadLine() ?? string.Empty;

    // Header bilgilerini parse etme
    var headers = new Dictionary<string, object?>();
    if (!string.IsNullOrEmpty(headerInput))
    {
        var headerPairs = headerInput.Split(',');
        foreach (var pair in headerPairs)
        {
            var keyValue = pair.Split(':');
            if (keyValue.Length == 2)
            {
                headers[keyValue[0].Trim()] = keyValue[1].Trim();
            }
        }
    }

    // BasicProperties oluşturma - RabbitMQ.Client 7.1.2 için
    var properties = new BasicProperties();
    properties.Headers = headers;

    // Mesaj gönderme - async versiyonu kullanıyoruz
    // 3. parametre mandatory
    await channel.BasicPublishAsync(
        exchange: "headerExchange",
        routingKey: "", // Header exchange'de routing key kullanılmaz
        false,
        body: message,
        basicProperties: properties
    );

    Console.WriteLine($"Mesaj gönderildi: {message.Length} byte, Headers: {string.Join(", ", headers.Select(h => $"{h.Key}:{h.Value}"))}");
}

// Programın kapanmaması için bekle
Console.Read();