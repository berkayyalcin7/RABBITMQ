using System.Text;
using RabbitMQ.Client;

namespace RabbitMQProject.Publisher
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            // RabbitMQ bağlantı yapılandırması
            var factory = new ConnectionFactory()
            {
                HostName = "localhost",
                Port = 5672,
                UserName = "admin",
                Password = "admin",
                VirtualHost = "/"
            };

            // Bağalantıyı aktifleştirme ve kanal oluşturma
            // Dispose etme yapılması gerekiyor.
            using IConnection connection = await factory.CreateConnectionAsync();
            // Kanal oluşturma 
            using IChannel channel = await connection.CreateChannelAsync();

            // Queue oluşturma. exclusive : true olarak ayarlanırsa, başka bir client bu queue'yi kullanamaz. 
            // autoDelete : true olarak ayarlanırsa, queue boş ise silinir.
            // Kuyruğun kalıcı olabilmesi için durable: true olarak ayarlanır.
            await channel.QueueDeclareAsync(queue: "exampleQueue", exclusive: false, autoDelete: false, durable: true);
            // BasicProperties oluşturma
            // Persistent: true olarak ayarlanırsa, mesajı kalıcı olarak kuyruğa atar.   
            var basicProperties = new BasicProperties
            {
                Persistent = true
            };

            // Random mesaj gönderme
            for (int i = 100; i < 200; i++)
            {
                await Task.Delay(200);
                // Mesaj gönderme - RabbitMQ kuyruğa atacağı mesajları byte array'ine çevirir ve kuyruğa atar.
                var message = Encoding.UTF8.GetBytes($"Hello, RabbitMQ! {i}");
                // BasicPublishAsync metodu ile mesajı kuyruğa atar.
                // exchange: "" olarak ayarlanırsa, direct exchange kullanılır.
                // routingKey: "exampleQueue" olarak ayarlanırsa, mesajı exampleQueue kuyruğuna atar.
                // body: message olarak ayarlanırsa, mesajı kuyruğa atar.
                await channel.BasicPublishAsync(exchange: "", routingKey: "exampleQueue", body: message);
            }
        }
    }
}
