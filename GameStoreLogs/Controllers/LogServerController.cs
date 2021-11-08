using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using Domain;
using GameStoreLogs.LogLogic;
using Microsoft.AspNetCore.Mvc;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace GameStoreLogs.Controllers
{
    [ApiController]
    [Route("api/logs")]
    public class LogServerController : ControllerBase
    {
        private readonly ILogsLogic _logsLogic;
        private List<Log> _logs;

        public LogServerController(ILogsLogic logsLogic)
        {
            this._logsLogic = logsLogic;
            _logs = new List<Log>();
        }

        [HttpGet]
        public IEnumerable<Log> GetAll()
        {
            GetLogsFromRabbtMq();
            return _logsLogic.GetAll();
        }

        private void GetLogsFromRabbtMq()
        {
            using var channel = new ConnectionFactory() {HostName = "localhost"}.CreateConnection().CreateModel();
            channel.QueueDeclare(queue: "log_queue",
                durable: false,
                exclusive: false,
                autoDelete: false,
                arguments: null);

            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                var log = JsonSerializer.Deserialize<Log>(message);
                _logsLogic.Add(log);
            };
            channel.BasicConsume(queue: "log_queue",
                autoAck: true,
                consumer: consumer);
        }
        
        
    }
}