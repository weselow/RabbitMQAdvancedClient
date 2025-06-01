using RabbitMQAdvancedClient.Models;

namespace RabbitMQAdvancedClient.Interfaces
{
    /// <summary>
    /// Интерфейс для работы с RabbitMQ.
    /// </summary>
    public interface IRabbitMqClient
    {
        /// <summary>
        /// Подписывается на очередь для получения сообщений.
        /// </summary>
        /// <typeparam name="T">Тип сообщения.</typeparam>
        /// <param name="queueName">Имя очереди.</param>
        /// <param name="onMessageReceived">Callback для обработки сообщений.</param>
        /// <param name="autoAck">Автоматическое подтверждение сообщений.</param>
        /// <returns>Task.</returns>
        Task SubscribeAsync<T>(string queueName, Func<T, MessageContext, Task> onMessageReceived, bool autoAck = true);

        /// <summary>
        /// Отписывается от очереди.
        /// </summary>
        /// <param name="queueName">Имя очереди.</param>
        /// <returns>Task.</returns>
        Task UnsubscribeAsync(string queueName);

        /// <summary>
        /// Публикует сообщение в очередь.
        /// </summary>
        /// <typeparam name="T">Тип сообщения.</typeparam>
        /// <param name="queueName">Имя очереди.</param>
        /// <param name="message">Сообщение.</param>
        /// <returns>Task.</returns>
        Task PublishAsync<T>(string queueName, T message);
    }
}