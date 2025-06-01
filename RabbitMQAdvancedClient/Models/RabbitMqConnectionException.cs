namespace RabbitMQAdvancedClient.Models
{
    /// <summary>
    /// Исключение, возникающее при проблемах с подключением к RabbitMQ.
    /// </summary>
    public class RabbitMqConnectionException : Exception
    {
        public RabbitMqConnectionException(string message) : base(message) { }
    }
}