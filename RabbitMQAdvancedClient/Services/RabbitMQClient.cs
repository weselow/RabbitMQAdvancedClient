using NLog;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQAdvancedClient.Interfaces;
using RabbitMQAdvancedClient.Models;
using System.Collections.Concurrent;
using System.Text.Json;

namespace RabbitMQAdvancedClient.Services
{
    /// <summary>
    /// Реализация клиента RabbitMQ.
    /// </summary>
    public class RabbitMqClient : IRabbitMqClient
    {
        private readonly IConnectionManager _connectionManager;
        private readonly Logger _logger = LogManager.GetCurrentClassLogger();

        // Словарь для отслеживания активных подписок
        private readonly ConcurrentDictionary<string, (IChannel, string)> _subscriptions = new();

        public RabbitMqClient(IConnectionManager connectionManager)
        {
            _connectionManager = connectionManager ?? throw new ArgumentNullException(nameof(connectionManager));
        }

        /// <inheritdoc />
        public async Task SubscribeAsync<T>(string queueName, Func<T, MessageContext, Task> onMessageReceived, bool autoAck = true)
        {
            if (string.IsNullOrWhiteSpace(queueName))
            {
                _logger.Error("Имя очереди не может быть пустым.");
                return;
            }

            if (_subscriptions.ContainsKey(queueName))
            {
                _logger.Info("Подписка на очередь {queueName} уже создана, не продолжаем.", queueName);
                return;
            }

            var connection = await _connectionManager.GetConnectionAsync();
            var channel = await connection.CreateChannelAsync();
            await CreateQueueAsync(channel, queueName);

            var consumer = new AsyncEventingBasicConsumer(channel);
            consumer.ReceivedAsync += async (model, ea) =>
            {
                try
                {
                    _logger.Debug("Событие Received сработало.");

                    var body = ea.Body.ToArray();
                    _logger.Debug($"Получены сырые данные: {BitConverter.ToString(body)}");

                    var message = JsonSerializer.Deserialize<T>(body);
                    if (message == null)
                    {
                        _logger.Warn("Не удалось десериализовать сообщение. Отправляем в DLQ.");
                        await channel.BasicNackAsync(ea.DeliveryTag, multiple: false, requeue: false); // Отправляем в DLQ
                        return;
                    }
                    _logger.Debug($"Десериализованное сообщение: {message}");

                    var context = new MessageContext(channel, ea.DeliveryTag);

                    //if (autoAck)
                    //{
                    //    await context.AckAsync(); // Автоматическое подтверждение
                    //}

                    await onMessageReceived(message, context);

                    if (!autoAck)
                    {
                        await context.AckAsync(); // Ручное подтверждение
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "Ошибка при обработке сообщения - {message}", ex.Message);
                    if (!autoAck)
                    {
                        await channel.BasicNackAsync(ea.DeliveryTag, multiple: false, requeue: true);
                    }
                }

            };
            
            var consumerTag = await channel.BasicConsumeAsync(queue: queueName, autoAck: autoAck, consumer: consumer);
            _subscriptions.TryAdd(queueName, (channel, consumerTag));

            _logger.Debug($"Подписка на очередь '{queueName}' выполнена.");
        }

        /// <inheritdoc />
        public async Task UnsubscribeAsync(string queueName)
        {
            if (_subscriptions.TryRemove(queueName, out var subsciption))
            {
                var (channel, tag) = subsciption;
                await channel.BasicCancelAsync(tag);
                await channel.CloseAsync();
                _logger.Debug($"Отписка от очереди '{queueName}' выполнена.");
            }
            else
            {
                _logger.Warn($"Не удалось найти подписку для очереди '{queueName}'.");
            }
        }

        /// <inheritdoc />
        public async Task PublishAsync<T>(string queueName, T message)
        {
            var connection = await _connectionManager.GetConnectionAsync();
            await using var channel = await connection.CreateChannelAsync();

           await CreateQueueAsync(channel, queueName);

            // Сериализуем сообщение
            var body = JsonSerializer.SerializeToUtf8Bytes(message);

            // Публикуем сообщение
            await channel.BasicPublishAsync(
                exchange: "",         // Используем дефолтный обменник
                routingKey: queueName,
                body: body
            );

            _logger.Debug($"Сообщение опубликовано в очередь '{queueName}'.");
        }

        /// <summary>
        /// Создаем очередь с параметрами по-умолчанию, если она не существует.
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="queueName"></param>
        /// <returns></returns>
        private async Task CreateQueueAsync(IChannel channel, string queueName)
        {
            // Создаем очередь, если она не существует
            await channel.QueueDeclareAsync(
                queue: queueName,
                durable: true,       // Очередь сохраняется при перезапуске RabbitMQ
                exclusive: false,    // Очередь доступна для других подключений
                autoDelete: false,   // Очередь не удаляется, если нет подписчиков
                arguments: null      // Дополнительные параметры (например, DLQ)
            );
        }
    }
}