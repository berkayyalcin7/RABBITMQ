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

Console.WriteLine("MessageTemplate Publisher başlatıldı. Bağlantı kuruldu.");
// TODO: Buraya kalan kodları yazabilirsiniz

#region P2P Tasarımı

//string queueName = "example-p2p-queue";

//await channel.QueueDeclareAsync(queueName, 
//    durable: true, 
//    exclusive: false, 
//    autoDelete: false);

//byte[] messageBody = 
//    Encoding.UTF8.GetBytes("Merhaba, RabbitMQ P2P Mesajı!");

//await channel.BasicPublishAsync(exchange: "", 
//    routingKey: queueName, 
//    body: messageBody);
#endregion

#region Pub/Sub Tasarımı - > Exchange'e gönderir bind edilmiş Bir çok consumer tarafından işlenir.

//string exchangeName = "example-pubsub-exchange";

//// İlgili exchange'e bind edilmiş queue'lar olmalı.
//await channel.ExchangeDeclareAsync(exchangeName, 
//    type: ExchangeType.Fanout, 
//    durable: true, 
//    autoDelete: false);


//// Birden fazla mesaj gönderelim

//for (int i = 1; i <= 5; i++)
//{
//    await Task.Delay(1000); // Her mesaj arasında 1 saniye bekle
//    string message = $"Merhaba, RabbitMQ Pub/Sub Mesajı {i}!";
//    byte[] messageBody = 
//        Encoding.UTF8.GetBytes(message);
//    await channel.BasicPublishAsync(exchange: exchangeName, 
//        routingKey: "", 
//        body: messageBody);
//    Console.WriteLine($"[x] Gönderilen Mesaj: {message}");
//}

#endregion


#region Work/Queue Tasarımı -> Direct Exchange kullanarak yayınlıyor 


//string queueName = "example-work-queue";

//// durable
//await channel.QueueDeclareAsync(queueName,
//    durable: true,
//    exclusive: false,
//    autoDelete: false);

//// Mesaj gönderme birden fazla
//for (int i = 1; i <= 15; i++)
//{
//    await Task.Delay(1000); // Her mesaj arasında 1 saniye bekle
//    string message = $"Merhaba, RabbitMQ Work/Queue Mesajı {i}!";
//    byte[] messageBody =
//        Encoding.UTF8.GetBytes(message);
//    await channel.BasicPublishAsync(exchange: "",
//        routingKey: queueName,
//        body: messageBody);
//    Console.WriteLine($"[x] Gönderilen Mesaj: {message}");
//}



#endregion

#region Request Response Tasarımı

string requestQueueName = "example-request-response-queue";

// Kuyruk tanımlama
await channel.QueueDeclareAsync(requestQueueName,
    durable: false,
    exclusive: false,
    autoDelete: false);

// Response Queue oluşturma
var replyQueue = await channel.QueueDeclareAsync(
    "", // Rastgele isim
    durable: false,
    exclusive: true,
    autoDelete: true);

// Correlation ID oluşturma
string correlationId = Guid.NewGuid().ToString();

#region Request Mesajı oluşturma ve gönderme

// Property oluşturma Async ile - RabbitMQ Yeni sürümünde nasılsa 
// BasicProperties kullanımı kaldırıldı.

// For ile mesajları oluşturma - correlationId ile birlikte gönderilir.
for (int i = 1; i <= 5; i++)
{
    await Task.Delay(1000);
    string message = $"Merhaba, RabbitMQ Request Response Mesajı {i}!";
    byte[] messageBody = Encoding.UTF8.GetBytes(message);
    await channel.BasicPublishAsync(
        exchange: "", 
        routingKey: requestQueueName,
        false,
        new BasicProperties 
        { CorrelationId = correlationId, 
            ReplyTo = replyQueue.QueueName },
        messageBody);

    
    Console.WriteLine($"[x] Gönderilen Mesaj: {message}");
}

#endregion

#region Response Kuyuruğu dinleme davranışı

var consumer = new AsyncEventingBasicConsumer(channel);

await channel.BasicConsumeAsync(
    queue: replyQueue.QueueName, // Response kuyruğu
    autoAck: true, // Otomatik onay
    consumer: consumer); // Tüketici ata

// Mesajları dinleme
consumer.ReceivedAsync += async (model, ea) =>
{
    // Correlation ID kontrolü
    if (ea.BasicProperties.CorrelationId == correlationId)
    {
        
        var body = ea.Body.ToArray();
        var message = Encoding.UTF8.GetString(body);
        Console.WriteLine($"[x] Alınan Response Mesajı: {message}");
    }
    await Task.Yield();
};
#endregion


#endregion



// Programın kapanmaması için bekle
Console.Read();

