using System.Net.Sockets;
using NLog;
using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;
using RabbitMQAdvancedClient.Interfaces;
using RabbitMQAdvancedClient.Models;

namespace RabbitMQAdvancedClient.Services
{
    /// <summary>
    /// Менеджер подключений к RabbitMQ.
    /// </summary>
    public class ConnectionManager : IConnectionManager
    {
        private readonly RabbitMqOptions _options;
        private IConnection? _connection;
        private readonly Logger _logger = LogManager.GetCurrentClassLogger();

        public ConnectionManager(RabbitMqOptions options)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
        }

        /// <inheritdoc />
        public async Task<IConnection> GetConnectionAsync()
        {
            if (_connection == null || !_connection.IsOpen)
            {
                _logger.Debug("Попытка установить новое соединение с RabbitMQ...");
                _connection = await CreateConnectionAsync();
            }
            return _connection;
        }

        /// <inheritdoc />
        public async Task CloseAsync()
        {
            if (_connection != null && _connection.IsOpen)
            {
                _logger.Debug("Закрытие соединения с RabbitMQ...");
                await _connection.CloseAsync();
                _connection.Dispose();
                _connection = null;
            }
        }

        private async Task<IConnection> CreateConnectionAsync()
        {
            var factory = new ConnectionFactory
            {
                HostName = _options.Host,
                Port = _options.Port,
                UserName = _options.Username,
                Password = _options.Password
            };

            int attempt = 0;
            while (attempt < _options.RetryCount || _options.RetryCount == -1)
            {
                try
                {
                    _logger.Debug($"Попытка подключения к RabbitMQ ({attempt + 1}/{_options.RetryCount})...");
                    return await factory.CreateConnectionAsync();
                }
                catch (BrokerUnreachableException ex)
                {
                    _logger.Warn(ex, "Не удалось подключиться к RabbitMQ.");
                   await  Task.Delay(TimeSpan.FromSeconds(_options.RetryDelay));
                    attempt++;
                }
                catch (SocketException ex)
                {
                    _logger.Warn(ex, "Ошибка сети при подключении к RabbitMQ.");
                    await Task.Delay(TimeSpan.FromSeconds(_options.RetryDelay));
                    attempt++;
                }
            }

            throw new RabbitMqConnectionException("Не удалось установить соединение с RabbitMQ после нескольких попыток.");
        }
    }
}