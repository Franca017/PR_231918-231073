using System;
using System.Threading;
using System.Threading.Tasks;
using Domain;
using GameStoreLogs.Context;
using GameStoreLogs.Services.RabbitMQService;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace GameStoreLogs.Services
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IBus _busControl;
        private readonly IServiceProvider _serviceProvider;
        
        public Worker(ILogger<Worker> logger, IServiceProvider serviceProvider){
            _serviceProvider = serviceProvider;
            _logger = logger;
            _busControl = RabbitHutch.CreateBus("localhost");
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await _busControl.ReceiveAsync<Log>(Queue.ProcessingQueueName, x =>
            {
                Task.Run(() => { ReceiveItem(x); }, stoppingToken);
            });
        }

        private void ReceiveItem(Log log)
        {
            _logger.LogInformation("Name: "+log.Message);
            try
            {
                using (var scope = _serviceProvider.CreateScope()) // Creamos un contexto de invocacion
                {
                    var db = new LogsContext(scope.ServiceProvider.GetRequiredService<DbContextOptions<LogsContext>>());
                    db.Logs.Add(log);
                    var addedItems = db.SaveChanges();
                    _logger.LogInformation($"Add {addedItems} items");
                }
            }
            catch (Exception e)
            {
                _logger.LogInformation($"Exception {e.Message} -> {e.StackTrace}");
            }
        }
    }
}