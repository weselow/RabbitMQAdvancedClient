using RabbitMQ.Client;

namespace RabbitMQAdvancedClient.Interfaces
{
    /// <summary>
    /// Интерфейс для управления подключением к RabbitMQ.
    /// </summary>
    public interface IConnectionManager
    {
        /// <summary>
        /// Получает активное соединение с RabbitMQ.
        /// </summary>
        /// <returns>Активное соединение.</returns>
        Task<IConnection> GetConnectionAsync();

        /// <summary>
        /// Закрывает соединение.
        /// </summary>
        Task CloseAsync();
    }
}