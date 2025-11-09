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

Console.WriteLine("MessageTemplate Consumer başlatıldı. Bağlantı kuruldu.");

// TODO: Buraya kalan kodları yazabilirsiniz

#region P2P tasarımı

//string queueName = "example-p2p-queue";

//await channel.QueueDeclareAsync(queueName,
//    durable: true,
//    exclusive: false,
//    autoDelete: false);

//var consumer = new AsyncEventingBasicConsumer(channel);

//await channel.BasicConsumeAsync(
//    queue: queueName,
//    autoAck: false,
//    consumer: consumer);

//consumer.ReceivedAsync += async (model, ea) =>
//{
//    var body = ea.Body.ToArray();
//    var message = Encoding.UTF8.GetString(body);
//    Console.WriteLine($"[x] Alınan Mesaj: {message}");
//    // Mesaj işlendiğinde onay gönder
//    await channel.BasicAckAsync(ea.DeliveryTag, multiple: false);
//};

#endregion

#region Pub/Sub Tasarımı - > Exchange'e gönderir bind edilmiş Bir çok consumer tarafından işlenir.

//string exchangeName = "example-pubsub-exchange";

//// İlgili exchange'e bind edilmiş queue'lar olmalı.
//await channel.ExchangeDeclareAsync(exchangeName,
//    type: ExchangeType.Fanout,
//    durable: true,
//    autoDelete: false);

//// Kuyruk tanımı
//var queue = await channel.QueueDeclareAsync();

//await channel.QueueBindAsync(
//    queue: queue.QueueName,
//    exchange: exchangeName,
//    routingKey: "");


//// Ölçeklendirme yapalım BasicQos ile
//await channel.BasicQosAsync(
//    prefetchSize: 0, // boyut sınırı yok
//    prefetchCount: 1, // her seferinde 1 mesaj al
//    global: false); // bu ayar consumer bazında geçerli olsun

//var consumer = new AsyncEventingBasicConsumer(channel);

//await channel.BasicConsumeAsync(
//    queue: queue.QueueName,
//    autoAck: false,
//    consumer: consumer);

//consumer.ReceivedAsync += async (model, ea) =>
//{
//    var body = ea.Body.ToArray();
//    var message = Encoding.UTF8.GetString(body);
//    Console.WriteLine($"[x] Alınan Mesaj: {message}");
//    // Mesaj işlendiğinde onay gönder
//    await channel.BasicAckAsync(ea.DeliveryTag, multiple: false);
//};

#endregion


#region Work/Queue Tasarımı -> Acknowledgment kullanarak mesajları işleme
//Tüm consumerlar aynı eşit yükü paylaşır.


//string queueName = "example-work-queue";

//await channel.QueueDeclareAsync(queueName,
//    durable: true,
//    exclusive: false,
//    autoDelete: false);

//var consumer = new AsyncEventingBasicConsumer(channel);

//await channel.BasicConsumeAsync(
//    queue: queueName,
//    autoAck: true,
//    consumer: consumer);

//await channel.BasicQosAsync(
//    prefetchSize: 0,
//    prefetchCount: 1,
//    global: false);

////Receive etme
//consumer.ReceivedAsync += async (model, ea) =>
//{
//    var body = ea.Body.ToArray();
//    var message = Encoding.UTF8.GetString(body);
//    Console.WriteLine($"[x] Alınan Mesaj: {message}");
//    // Mesaj işlendiğinde onay gönder
//    await channel.BasicAckAsync(ea.DeliveryTag, multiple: false);
//};




#endregion

#region Request Response Tasarımı

// queue tanımı
string queueName = "example-request-response-queue";

await channel.QueueDeclareAsync(queueName,
    durable: false,
    exclusive: false,
    autoDelete: false);

// Tüketici tanımı
var consumer = new AsyncEventingBasicConsumer(channel);
// Mesaj alındığında çalışacak event
await channel.BasicConsumeAsync(
    queue: queueName,
    autoAck: true,
    consumer: consumer);

// Mesajları dinleme
consumer.ReceivedAsync += async (model, ea) =>
{
    var body = ea.Body.ToArray();
    var message = Encoding.UTF8.GetString(body);
    Console.WriteLine($"[x] Alınan Mesaj: {message}");
    // Response mesajı oluşturma
    string responseMessage = $"Response: {message}";
    byte[] responseBody = Encoding.UTF8.GetBytes(responseMessage);

    // Response mesajını gönderme
    await channel.BasicPublishAsync(
        exchange: "",
        routingKey: ea.BasicProperties.ReplyTo, // ReplyTo kuyruğuna gönder
        false,
        new BasicProperties
        {
            CorrelationId = ea.BasicProperties.CorrelationId
        },
        responseBody);
    Console.WriteLine($"[x] Gönderilen Response Mesajı: {responseMessage}");
};


#endregion

// Programın kapanmaması için bekle
Console.Read();

