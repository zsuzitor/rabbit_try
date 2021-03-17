using System;
using RabbitMQ.Client;
using System.Text;

namespace ProducerSimple
{
    class Program
    {
        //install https://www.erlang.org/downloads
        //install https://www.rabbitmq.com/install-windows.html

        //https://github.com/rabbitmq/rabbitmq-tutorials/blob/master/dotnet
        static void Main(string[] args)
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {

                //объявление должно быть одинаковым и в продьюсерах и в консьюмерах
                channel.QueueDeclare(queue: "hello",
                    durable: false,//при true сохранит очередь при рестарте сервиса
                    exclusive: false, 
                    autoDelete: false,
                    arguments: null);

                var properties = channel.CreateBasicProperties();
                properties.Persistent = true;//это заставит(не 100% гарантия) сохраниться необработанные сообщения после растарта сервиса. но думаю очередь надо помечать durable для сохранения. для 100% гарантии вот ссылка какая то https://www.rabbitmq.com/confirms.html

                for (int i = 0; i < 10; i++)
                {
                    string message = $"Hello World {i}!";
                    var body = Encoding.UTF8.GetBytes(message);

                    //properties - can be null
                    channel.BasicPublish(exchange: "", routingKey: "hello", basicProperties: properties, body: body);
                    Console.WriteLine(" [x] Sent {0}", message);
                }
               
            }

            Console.WriteLine(" Press [enter] to exit.");
            Console.ReadLine();
        }
    }
}
