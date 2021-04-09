using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Text;

namespace ConsumerSimple
{
    class ConsumerExchange
    {
        public static void Main()
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                //Fanout - раскидывает каждое сообщение всем подписчикам
                //direct можем подписаться по ключу, см QueueBind.routingKey
                channel.ExchangeDeclare(exchange: "logs", 
                    type: ExchangeType.Fanout);

                var queueName = channel.QueueDeclare().QueueName;//имя ренерируется рандомно внутри, мы напрямую в очередь не стучимся, имя нам не нужно
                channel.QueueBind(queue: queueName,
                                  exchange: "logs",
                                  routingKey: "");//см BasicPublish.routingKey . одну очередь можно забиндить на несколько ключей

                Console.WriteLine(" [*] Waiting for logs.");

                var consumer = new EventingBasicConsumer(channel);
                consumer.Received += (model, ea) =>
                {
                    var body = ea.Body.ToArray();
                    var message = Encoding.UTF8.GetString(body);
                    var rk = ea.RoutingKey;
                    Console.WriteLine(" [x] {0}", message);
                };
                channel.BasicConsume(queue: queueName,
                                     autoAck: true,
                                     consumer: consumer);

                Console.WriteLine(" Press [enter] to exit.");
                Console.ReadLine();
            }
        }
    }
}
