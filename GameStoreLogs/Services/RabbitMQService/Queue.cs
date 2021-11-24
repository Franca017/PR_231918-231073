namespace GameStoreLogs.Services.RabbitMQService
{
    public class Queue
    {
        public static string ProcessingQueueName { get; } = "log_queue"; // Nombre de la cola de mensajes
    }
}