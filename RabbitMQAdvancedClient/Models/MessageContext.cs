using RabbitMQ.Client;

namespace RabbitMQAdvancedClient.Models
{
    /// <summary>
    /// Контекст сообщения для управления подтверждениями.
    /// </summary>
    public class MessageContext
    {
        private readonly IChannel _channel;
        private readonly ulong _deliveryTag;

        public MessageContext(IChannel channel, ulong deliveryTag)
        {
            _channel = channel;
            _deliveryTag = deliveryTag;
        }

        /// <summary>
        /// Подтверждает успешную обработку сообщения.
        /// </summary>
        public async Task AckAsync()
        {
            await _channel.BasicAckAsync(_deliveryTag, multiple: false);
        }

        /// <summary>
        /// Отклоняет сообщение (с возможностью повторной отправки).
        /// </summary>
        /// <param name="requeue">Повторно поставить в очередь.</param>
        public async Task NackAsync(bool requeue)
        {
            await _channel.BasicNackAsync(_deliveryTag, multiple: false, requeue: requeue);
        }
    }
}