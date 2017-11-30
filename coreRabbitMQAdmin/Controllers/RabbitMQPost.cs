using System.Text;
using Newtonsoft.Json;
using RabbitMQ.Client;

public class RabbitMQPost
{
    public Stoc data;
    public RabbitMQPost(Stoc _data){
        this.data=_data;
    }
    public string Post()
    {
        var factory = new ConnectionFactory() { HostName = "localhost" };
        using (var connection = factory.CreateConnection())
        using (var channel = connection.CreateModel())
        {
            channel.QueueDeclare(queue: data.Name,
                                 durable: false,
                                 exclusive: false,
                                 autoDelete: false,
                                 arguments: null);

            var stocData = JsonConvert.SerializeObject(data);
            var body = Encoding.UTF8.GetBytes(stocData);

            channel.BasicPublish(exchange: "",
                                 routingKey: data.Name,
                                 basicProperties: null,
                                 body: body);
            return $"[x] Sent {data.Name}";
        }
    }
}