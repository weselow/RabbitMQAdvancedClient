namespace RabbitMQAdvancedClient.Models
{
    /// <summary>
    /// Параметры подключения к RabbitMQ.
    /// </summary>
    public class RabbitMqOptions
    {
        /// <summary>
        /// Хост RabbitMQ сервера.
        /// </summary>
        public string Host { get; set; } = "localhost";

        /// <summary>
        /// Порт RabbitMQ сервера.
        /// </summary>
        public int Port { get; set; } = 5672;

        /// <summary>
        /// Имя пользователя.
        /// </summary>
        public string Username { get; set; } = "guest";

        /// <summary>
        /// Пароль пользователя.
        /// </summary>
        public string Password { get; set; } = "guest";

        /// <summary>
        /// Количество попыток переподключения.
        /// </summary>
        public int RetryCount { get; set; } = 3;

        /// <summary>
        /// Задержка между попытками переподключения (в секундах).
        /// </summary>
        public int RetryDelay { get; set; } = 5;

        /// <summary>
        /// Формирует строку подключения из параметров.
        /// </summary>
        /// <returns>Строка подключения.</returns>
        public string GetConnectionString()
        {
            return $"amqp://{Username}:{Password}@{Host}:{Port}";
        }
    }
}