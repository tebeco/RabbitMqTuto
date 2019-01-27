using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Send
{
    public class Program
    {
        public static async Task Main()
        {
            var cts = new CancellationTokenSource();
            Console.CancelKeyPress += Console_CancelKeyPress;
            var ct = cts.Token;

            var connectionFactory = new ConnectionFactory { HostName = "localhost" };
            using (var connection = connectionFactory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.QueueDeclare(queue: "hello", durable: true, exclusive: false, autoDelete: false, arguments: null);
                var properties = channel.CreateBasicProperties();
                properties.Persistent = true;

                var itemIndex = 0;
                var delay = TimeSpan.FromMilliseconds(700);
                var delta = TimeSpan.FromMilliseconds(20);
                while (!ct.IsCancellationRequested)
                {
                    string message = $"[{itemIndex++:d3}] Hello World!";
                    var body = Encoding.UTF8.GetBytes(message);

                    channel.BasicPublish(exchange: "", routingKey: "hello", basicProperties: properties, body: body);
                    Console.WriteLine(" [x] Sent {0}", message);


                    await Task.Delay(delay, ct).ContinueWith(tsk => { });
                    while (Console.KeyAvailable)
                    {
                        var key = Console.ReadKey(false);
                        switch (key.Key)
                        {
                            case ConsoleKey.UpArrow:
                                delay = delay.Add(delta);
                                break;
                            case ConsoleKey.DownArrow:
                                delay = delay.Subtract(delta);
                                break;
                        }

                        if (delay <= delta)
                            delay = delta;
                        Console.WriteLine($"New delay between messge: {delay.TotalMilliseconds}ms");
                    }
                }
            }

            void Console_CancelKeyPress(object sender, ConsoleCancelEventArgs e)
            {
                e.Cancel = true;
                cts.Cancel();
            }
        }

    }
}
