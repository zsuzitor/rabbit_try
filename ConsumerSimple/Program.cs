using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;
using System.Threading;

namespace ConsumerSimple
{
    class Program
    {
        static void Main(string[] args)
        {
            //message acknowledgment - "ACK" - если консьюмер забрал сообщение но упал при обработке, можно сделать
            //что бы это сообщение не удалялось из очереди и было "переобработано" свободным консьюмером. см BasicAck

            var factory = new ConnectionFactory() { HostName = "localhost" };
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.QueueDeclare(queue: "hello", durable: false, exclusive: false, autoDelete: false, arguments: null);//из продьюсера
                channel.BasicQos(0, 1, false);//без этой штуки рэббит просто раскидывает по очереди(порядку)! все сообщения и не смотрит кто там занят а кто нет. это позволяет закидывать сообщения только свободным консьюмерам, но может переполниться очередь

                Console.WriteLine(" [*] Waiting for messages.");

                var consumer = new EventingBasicConsumer(channel);
                consumer.Received += (model, ea) =>
                {
                    var body = ea.Body.ToArray();
                    var message = Encoding.UTF8.GetString(body);
                    var rnd = new Random();
                    //Thread.Sleep(rnd.Next(100,5000));
                    Thread.Sleep(5000);
                    Console.WriteLine(" [x] Received {0}", message);

                    // Note: it is possible to access the channel via
                    //       ((EventingBasicConsumer)sender).Model here
                    //указываем что мы обработали сообщение полностью, и его можно удалить из очереди. не будет работать пока установлен флаг BasicConsume->autoAck->true
                    channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);

                };
                channel.BasicConsume(queue: "hello",
                    autoAck: false, 
                    consumer: consumer);

                Console.WriteLine(" Press [enter] to exit.");
                Console.ReadLine();
            }


        }
    }
}
