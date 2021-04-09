using System;
using RabbitMQ.Client;
using System.Text;

namespace ProducerSimple
{
    class Program
    {
        //список туторов https://www.rabbitmq.com/getstarted.html

        //install https://www.erlang.org/downloads
        //install https://www.rabbitmq.com/install-windows.html

        //https://github.com/rabbitmq/rabbitmq-tutorials/blob/master/dotnet
        static void Main(string[] args)
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                const string queueName = "hello";
                const string exchange = "logs";

                //объявляем очередь, НО только если не хотим настраивать Exchange, иначе тут надо объявить Exchange
                //объявление должно быть одинаковым и в продьюсерах и в консьюмерах
                channel.QueueDeclare(queue: queueName,
                    durable: false,//при true сохранит очередь при рестарте сервиса
                    exclusive: false, 
                    autoDelete: false,
                    arguments: null);

                //это должно быть ВМЕСТО QueueDeclare, и в продьюсере должно быть обязательно тк в таком случае консьюмеров может впринципе не быть
                //channel.ExchangeDeclare(exchange: exchange, type: ExchangeType.Fanout);


                var properties = channel.CreateBasicProperties();
                properties.Persistent = true;//это заставит(не 100% гарантия) сохраниться необработанные сообщения после растарта сервиса. но очередь надо помечать durable для сохранения. для 100% гарантии вот ссылка какая то https://www.rabbitmq.com/confirms.html

                for (int i = 0; i < 10; i++)
                {
                    string message = $"Hello World {i}!";
                    var body = Encoding.UTF8.GetBytes(message);

                    //properties - can be null

                    //если очередь
                    channel.BasicPublish(exchange: "", routingKey: queueName, basicProperties: properties, body: body);

                    //если exchange
                    //channel.BasicPublish(exchange: exchange,
                    //routingKey: "",//это ключ по которому сообщения будет получать конкретная очередь. когда ExchangeType.direct\topic ... на это значение смотрит QueueBind.routingKey
                    //basicProperties: null, body: body);

                    Console.WriteLine(" [x] Sent {0}", message);
                }
               
            }

            Console.WriteLine(" Press [enter] to exit.");
            Console.ReadLine();
        }
    }
}
