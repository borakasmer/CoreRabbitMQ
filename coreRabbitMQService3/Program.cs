using System;
using System.Text;
using Microsoft.AspNetCore.SignalR.Client;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace coreRabbitMQService3
{
    class Program
    {
        static void Main(string[] args)
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                //Trigger The SignalR
                var connectionSignalR = new HubConnectionBuilder()
                .WithUrl("http://localhost:1453/stochub")
                .WithConsoleLogger()
                .Build();
                connectionSignalR.StartAsync();

                channel.QueueDeclare(queue: "NetasTelekom",
                                     durable: false,
                                     exclusive: false,
                                     autoDelete: false,
                                     arguments: null);

                var consumer = new EventingBasicConsumer(channel);
                consumer.Received += (model, ea) =>
                {
                    var body = ea.Body;
                    var data = Encoding.UTF8.GetString(body);
                    Stoc stoc = JsonConvert.DeserializeObject<Stoc>(data);
                    Console.WriteLine(" [x] Received {0}", stoc.Name + " : " + stoc.Value);

                    connectionSignalR.InvokeAsync("PushNotify", stoc);
                    //-------------------------

                };
                channel.BasicConsume(queue: "NetasTelekom",
                                     autoAck: true,
                                     consumer: consumer);

                Console.WriteLine(" Press [enter] to exit.");
                Console.ReadLine();
            }
        }
    }
}
