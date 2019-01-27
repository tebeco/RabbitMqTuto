using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Receive
{
    public class Program
    {
        public static async Task Main()
        {
            var cts = new CancellationTokenSource();
            Console.CancelKeyPress += Console_CancelKeyPress;
            var ct = cts.Token;

            var connectionFactory = new ConnectionFactory() { HostName = "localhost" };
            using (var connection = connectionFactory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.QueueDeclare(queue: "hello", durable: true, exclusive: false, autoDelete: false, arguments: null);

                var consumer = new EventingBasicConsumer(channel);
                consumer.Received += Consumer_Received;

                channel.BasicConsume(queue: "hello", autoAck: false, consumer: consumer);




                // Await for Ctrl+C
                await Task.Delay(-1, ct).ContinueWith(tsk => { });
            }

            void Console_CancelKeyPress(object sender, ConsoleCancelEventArgs e)
            {
                e.Cancel = true;
                cts.Cancel();
            }
        }

        private static void Consumer_Received(object sender, BasicDeliverEventArgs e)
        {
            var body = e.Body;
            var message = Encoding.UTF8.GetString(body);
            Console.WriteLine(" [x] Received {0}", message);

            var channel = (IModel)sender;
            //channel.BasicAck(e.DeliveryTag, false);
            channel.BasicAck(deliveryTag: e.DeliveryTag, multiple: false);

        }
    }
}