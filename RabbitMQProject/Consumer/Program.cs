using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;


ConnectionFactory factory = new ConnectionFactory();
factory.HostName = "localhost";
factory.UserName = "admin";
factory.Password = "admin";
factory.VirtualHost = "/";

// Bağlantı oluşturma
using IConnection connection = await factory.CreateConnectionAsync();
// Kanal oluşturma
using IChannel channel = await connection.CreateChannelAsync();

// Queue oluşturma - Publisher'dan gönderilen mesajları almak için kullanılır.
// Publisher ile aynı yapıda olmalıdır.
// Durable : Publisherda neyse buradada o olmalı
await channel.QueueDeclareAsync(queue: "exampleQueue", exclusive: false, autoDelete: false,durable:true);

// Mesaj alma
var consumer = new AsyncEventingBasicConsumer(channel);
// Mesaj alma işlemi başlatılır.
// autoAck: true olarak ayarlanırsa, mesajı aldıktan sonra otomatik olarak silinir.
// false olarak ayarlanırsa, mesajı aldıktan sonra manuel olarak silinir.
await channel.BasicConsumeAsync(queue: "exampleQueue", autoAck:false, consumer: consumer);
// BasicQos parametresi - Mesaj akışını kontrol eder ve consumer'ın kaç mesaj alacağını belirler
await channel.BasicQosAsync(0,1,false); // 0: prefetch size (mesaj boyutu sınırı - 0=sınırsız), 1: prefetch count (aynı anda alınacak mesaj sayısı), false: global (sadece bu consumer için geçerli)

consumer.ReceivedAsync += async (model, ea) =>
{
    // Mesajı byte array'ine çevirir.
    // Mesajı string'e çevirir.
    var message = Encoding.UTF8.GetString(ea.Body.Span);
    // Mesajı ekrana yazdırır.
    Console.WriteLine("Mesaj alındı: {0}", message);

    //Mesajı silme - Başarılı işlemlerde
    await channel.BasicAckAsync(ea.DeliveryTag, false);

    // ===== CANCEL, REJECT VE NACK ARASINDAKİ FARKLAR =====

    // 1. CANCEL (BasicCancelAsync):
    // - Consumer'ı iptal eder ve kapatır
    // - Sadece tek bir mesaj için değil, tüm consumer'ı kapatır
    // - Mesajı silmez, sadece consumer'ı durdurur
    // - Kullanım: await channel.BasicCancelAsync(consumerTag);
    // - Genellikle consumer'ı tamamen kapatmak için kullanılır

    // 2. REJECT (BasicRejectAsync):
    // - Tek bir mesajı reddeder
    // - Mesajı siler veya Dead Letter Queue'ya gönderir
    // - requeue parametresi ile mesajın tekrar kuyruğa atılıp atılmayacağını belirler
    // - Kullanım: await channel.BasicRejectAsync(ea.DeliveryTag, requeue: false);
    // - requeue: true -> mesajı tekrar kuyruğa atar
    // - requeue: false -> mesajı siler veya DLQ'ya gönderir

    // 3. NACK (BasicNackAsync):
    // - Negative Acknowledgment (Olumsuz Onay)
    // - Tek veya birden fazla mesajı reddeder
    // - multiple parametresi ile birden fazla mesajı reddedebilir
    // - requeue parametresi ile mesajın tekrar kuyruğa atılıp atılmayacağını belirler
    // - Kullanım: await channel.BasicNackAsync(ea.DeliveryTag, multiple: false, requeue: true);
    // - multiple: true -> o mesajdan önceki tüm mesajları da reddeder
    // - multiple: false -> sadece o mesajı reddeder
    // - requeue: true -> mesajı tekrar kuyruğa atar
    // - requeue: false -> mesajı siler veya DLQ'ya gönderir

    // ===== ÖRNEK KULLANIMLAR =====

    // Başarılı işlem - mesajı onayla
    // await channel.BasicAckAsync(ea.DeliveryTag, false);

    // Mesajı reddet ve tekrar kuyruğa at (retry için)
    // await channel.BasicRejectAsync(ea.DeliveryTag, requeue: true);

    // Mesajı reddet ve sil (DLQ'ya gönder)
    // await channel.BasicRejectAsync(ea.DeliveryTag, requeue: false);

    // Mesajı NACK ile reddet ve tekrar kuyruğa at
    // await channel.BasicNackAsync(ea.DeliveryTag, multiple: false, requeue: true);

    // Mesajı NACK ile reddet ve sil
    // await channel.BasicNackAsync(ea.DeliveryTag, multiple: false, requeue: false);

    // Birden fazla mesajı NACK ile reddet
    // await channel.BasicNackAsync(ea.DeliveryTag, multiple: true, requeue: false);

    // Consumer'ı tamamen kapat
    // await channel.BasicCancelAsync(consumerTag);

};

// Mesaj alma işlemi beklenir.
Console.Read();