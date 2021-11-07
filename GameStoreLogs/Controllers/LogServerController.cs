using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace GameStoreLogs.Controllers
{
    [ApiController]
    [Route("api/logs")]
    public class LogServerController : ControllerBase
    {
        private readonly ILogger<LogServerController> _logger;
        private List<Log> _logs;

        public LogServerController(ILogger<LogServerController> logger)
        {
            _logger = logger;
            _logs = new List<Log>();
        }

        [HttpGet]
        public IEnumerable<Log> GetAll()
        {
            GetLogsFromRabbtMq();
            return _logs;
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
                _logs.Add(log);
            };
            channel.BasicConsume(queue: "log_queue",
                autoAck: true,
                consumer: consumer);
        }
        
        
    }
}