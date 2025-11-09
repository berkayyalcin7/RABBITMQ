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


string queueName = "example-work-queue";




#endregion


// Programın kapanmaması için bekle
Console.Read();

