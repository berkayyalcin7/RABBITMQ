using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;


ConnectionFactory factory = new ConnectionFactory();
factory.HostName = "localhost";
factory.UserName = "admin";
factory.Password = "admin123";
factory.VirtualHost = "/";

// Bağlantı oluşturma
using IConnection connection = await factory.CreateConnectionAsync();
// Kanal oluşturma
using IChannel channel = await connection.CreateChannelAsync();

// Queue oluşturma - Publisher'dan gönderilen mesajları almak için kullanılır.
// Publisher ile aynı yapıda olmalıdır.
await channel.QueueDeclareAsync(queue: "exampleQueue", exclusive: false, autoDelete: false);

// Mesaj alma
var consumer = new AsyncEventingBasicConsumer(channel);
// Mesaj alma işlemi başlatılır.
// autoAck: true olarak ayarlanırsa, mesajı aldıktan sonra otomatik olarak silinir.
// false olarak ayarlanırsa, mesajı aldıktan sonra manuel olarak silinir.
await channel.BasicConsumeAsync(queue: "exampleQueue", false, consumer: consumer);

consumer.ReceivedAsync += async (model, ea) =>
{
    // Mesajı byte array'ine çevirir.
    // Mesajı string'e çevirir.
    var message = Encoding.UTF8.GetString(ea.Body.Span);
    // Mesajı ekrana yazdırır.
    Console.WriteLine("Mesaj alındı: {0}", message);
};
// Mesaj alma işlemi beklenir.
Console.Read();