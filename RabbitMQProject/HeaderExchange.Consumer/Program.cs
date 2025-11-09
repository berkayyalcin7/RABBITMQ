using RabbitMQ.Client;
using RabbitMQ.Client.Events;
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

Console.WriteLine("HeaderExchange Consumer başlatıldı. Bağlantı kuruldu.");

await channel.ExchangeDeclareAsync(
    exchange: "headerExchange",
    type: ExchangeType.Headers
);

Console.WriteLine("Dinlenecek Header bilgilerini belirtiniz (örn: priority:high,type:notification)");
string headerInput = Console.ReadLine() ?? string.Empty;

Console.WriteLine("x-match konfigürasyonunu seçiniz (all/any):");
Console.WriteLine("all: Tüm header'ların eşleşmesi gerekir");
Console.WriteLine("any: Herhangi bir header'ın eşleşmesi yeterlidir");
string xMatchInput = Console.ReadLine() ?? "all";

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

// x-match konfigürasyonunu ekleme
headers["x-match"] = xMatchInput.ToLower() == "any" ? "any" : "all";

var queueName = await channel.QueueDeclareAsync();

// Exchange ile Queue bağlama (Header exchange için)
await channel.QueueBindAsync(
    queue: queueName.QueueName,
    exchange: "headerExchange",
    routingKey: "", // Header exchange'de routing key kullanılmaz
    arguments: headers // Header bilgileri arguments olarak geçilir
);

Console.WriteLine($"Queue '{queueName.QueueName}' oluşturuldu ve headerExchange ile bağlandı.");
Console.WriteLine($"Dinlenen Headers: {string.Join(", ", headers.Where(h => h.Key != "x-match").Select(h => $"{h.Key}:{h.Value}"))}");
Console.WriteLine($"x-match konfigürasyonu: {headers["x-match"]}");

// Consumer oluşturma
AsyncEventingBasicConsumer consumer = new AsyncEventingBasicConsumer(channel);

// Consumer'ı başlatma
await channel.BasicConsumeAsync(
    queue: queueName.QueueName,
    autoAck: true,
    consumer: consumer
);

// Mesaj alma ve tüketme
consumer.ReceivedAsync += (model, ea) =>
{
    var message = Encoding.UTF8.GetString(ea.Body.Span);
    var messageHeaders = ea.BasicProperties.Headers ?? new Dictionary<string, object?>();
    
    Console.WriteLine("Mesaj alındı: {0}", message);
    Console.WriteLine("Mesaj Headers: {0}", string.Join(", ", messageHeaders.Select(h => $"{h.Key}:{h.Value}")));
    Console.WriteLine("---");
    return Task.CompletedTask;
};

Console.WriteLine("HeaderExchange Consumer hazır. Mesajları dinliyor...");
Console.WriteLine("\n=== Header Exchange Örnekleri ===");
Console.WriteLine("Header örnekleri:");
Console.WriteLine("- priority:high,type:notification");
Console.WriteLine("- priority:low,type:email");
Console.WriteLine("- priority:medium,type:sms");
Console.WriteLine("- type:notification");
Console.WriteLine("- priority:high");
Console.WriteLine("\n=== x-match Konfigürasyonu ===");
Console.WriteLine("all: Mesajın TÜM header'ları consumer'ın header'larıyla eşleşmeli");
Console.WriteLine("any: Mesajın HERHANGİ BİR header'ı consumer'ın header'larıyla eşleşmeli");
Console.WriteLine("\nÖrnek Senaryolar:");
Console.WriteLine("Consumer: priority:high,type:notification, x-match:all");
Console.WriteLine("Publisher: priority:high,type:notification -> ✅ Eşleşir");
Console.WriteLine("Publisher: priority:high,type:email -> ❌ Eşleşmez");
Console.WriteLine("\nConsumer: priority:high,type:notification, x-match:any");
Console.WriteLine("Publisher: priority:high,type:email -> ✅ Eşleşir (priority eşleşti)");
Console.WriteLine("Publisher: priority:low,type:notification -> ✅ Eşleşir (type eşleşti)");

// Programın kapanmaması için bekle
Console.Read();