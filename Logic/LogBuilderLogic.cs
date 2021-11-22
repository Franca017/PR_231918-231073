using System;
using System.Text;
using System.Text.Json;
using Domain;
using LogicInterface;
using RabbitMQ.Client;

namespace Logic
{
    public class LogBuilderLogic : ILogBuilderLogic
    {
        private readonly IModel _channel;
        
        public LogBuilderLogic()
        {
            _channel = new ConnectionFactory() {HostName = "localhost"}.CreateConnection().CreateModel();
            _channel.QueueDeclare(queue: "log_queue",
                durable: false,
                exclusive: false,
                autoDelete: false,
                arguments: null);
        }
        
        private bool SendLog(Log log)
        {
            var stringLog = JsonSerializer.Serialize(log);
            bool returnVal;
            try
            {
                var body = Encoding.UTF8.GetBytes(stringLog);
                _channel.BasicPublish(exchange: "",
                    routingKey: "log_queue",
                    basicProperties: null,
                    body: body);
                returnVal = true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                returnVal = false;
            }

            return returnVal;
        }

        public async void BuildLog(Game game, string user, string action, string message)
        {
            var newLog = new Log(game.Id, game.Title , user, DateTime.Now, action, message);
            SendLog(newLog);
        }
    }
}