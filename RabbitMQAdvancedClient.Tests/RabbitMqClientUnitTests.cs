using RabbitMQAdvancedClient.Models;
using RabbitMQAdvancedClient.Services;

namespace RabbitMQAdvancedClient.Tests;

[TestClass]
public class RabbitMqClientTests
{
    private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

    [TestMethod]
    public async Task TestPublishAndSubscribe()
    {
        // Arrange
        var options = new RabbitMqOptions
        {
            Host = "10.20.0.185",
            Port = 5672,
            Username = "root",
            Password = "toor"
        };

        var connectionManager = new ConnectionManager(options);
        var rabbitMqClient = new RabbitMqClient(connectionManager);

        string queueName = "test_queue";
        string testMessage = "Hello, RabbitMQ!";

        bool messageReceived = false;
        var cts = new CancellationTokenSource(TimeSpan.FromSeconds(20)); // Таймаут 10 секунд

        // Подписываемся на очередь
        await rabbitMqClient.SubscribeAsync<string>(queueName, (msg, context) =>
        {
            Assert.AreEqual(testMessage, msg);
            Logger.Debug($"Сообщение получено: {msg}");
            messageReceived = true;
            cts.Cancel(); // Останавливаем ожидание при получении сообщения
            return Task.CompletedTask;
        }, autoAck: true);

        // Ждём, пока подписчик будет готов
        await Task.Delay(1000, cts.Token);

        // Публикуем сообщение
        await rabbitMqClient.PublishAsync(queueName, testMessage);
        Logger.Debug($"Сообщение отправлено в очередь '{queueName}': {testMessage}");

        // Ждем получения сообщения или таймаута
        try
        {
            await Task.Delay(Timeout.Infinite, cts.Token);
        }
        catch (TaskCanceledException)
        {
            // Expected behavior
        }

        // Assert
        Assert.IsTrue(messageReceived, "Сообщение не было получено в течение времени ожидания.");
    }
}