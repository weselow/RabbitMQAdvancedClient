# RabbitMQAdvancedClient

![Icon](RabbitMQAdvancedClient/Resources/icon.png)

����������� ����������-������ ��� ������ � RabbitMQ � .NET �����������. ������������ ������� � �������� API ��� ���������� � �������� �� ���������, � ���������� ��������� ������� �����������, ����������� � ���������� ��������������� ���������.

## ����������

- [�����������](#�����������)
- [���������](#���������)
- [���������](#���������)
  - [��������� �����������](#���������-�����������)
- [�������������](#�������������)
  - [���������� � Dependency Injection](#����������-�-dependency-injection)
  - [���������� ���������](#����������-���������)
  - [�������� �� ���������](#��������-��-���������)
  - [���������� ��������������� ���������](#����������-���������������-���������)
- [�������](#�������)
  - [������� ������](#�������-������)
  - [����������� ������](#�����������-������)
- [����������](#����������)
- [��������](#��������)

## �����������

- ������� � ������� API ��� ������ � RabbitMQ
- ������ ������������� (async/await)
- �������������� ��������� ����������� ��� ������ ����������
- ���������� ��������� ����������� � NLog
- ������������/�������������� JSON ���������
- ���������� ��������������� ��������� (Ack/Nack)
- ������� ���������� � .NET Dependency Injection
- �������������� ��������� (generic ����)

## ���������

��� ��������� ������ RabbitMQAdvancedClient ����������� NuGet Package Manager:
dotnet add package RabbitMQAdvancedClient
## ���������

### ��������� �����������

��� ����������� � RabbitMQ ������� ���������� ��������� ��������� ����������� � ������� ������ `RabbitMqOptions`:
```csharp
var options = new RabbitMqOptions
{
    Host = "localhost",     // ���� ������� RabbitMQ (�� ��������� "localhost")
    Port = 5672,            // ���� ������� RabbitMQ (�� ��������� 5672)
    Username = "guest",     // ��� ������������ (�� ��������� "guest")
    Password = "guest",     // ������ ������������ (�� ��������� "guest")
    RetryCount = 3,         // ���������� ������� ��������������� (�� ��������� 3)
    RetryDelay = 5          // �������� ����� ��������� � �������� (�� ��������� 5)
};
```
## �������������

### ���������� � Dependency Injection

���������� ������������ ���������� � .NET Dependency Injection. �������� ��������� ��� � ����� `ConfigureServices` ������ `Startup.cs` ��� � ������������ ��������:

```csharp
// ����������� � ������� �������� ���������
services.AddRabbitMq(options => 
{
    options.Host = "rabbitmq.example.com";
    options.Port = 5672;
    options.Username = "user";
    options.Password = "password";
    options.RetryCount = 5;
    options.RetryDelay = 10;
});

// ��� ����������� � �������������� �������������� ������������ �������
var rabbitOptions = new RabbitMqOptions
{
    Host = "rabbitmq.example.com",
    // ������ ���������...
};
services.AddRabbitMq(rabbitOptions);
```

����� ����������� �� ������ �������� `IRabbitMqClient` � ���� �������:

```csharp
public class MyService
{
    private readonly IRabbitMqClient _rabbitMqClient;

    public MyService(IRabbitMqClient rabbitMqClient)
    {
        _rabbitMqClient = rabbitMqClient;
    }

    // ����������� _rabbitMqClient ��� ���������� � �������� �� ���������
}
```
### ���������� ���������

��� ���������� ��������� ����������� ����� `PublishAsync<T>`:

```csharp
// ����������� ������ ���������
public class OrderCreatedMessage
{
    public int OrderId { get; set; }
    public decimal Amount { get; set; }
    public DateTime CreatedAt { get; set; }
}

// ���������� ���������
await _rabbitMqClient.PublishAsync("orders.created", new OrderCreatedMessage
{
    OrderId = 12345,
    Amount = 99.99m,
    CreatedAt = DateTime.UtcNow
});
```

### �������� �� ���������

��� �������� �� ��������� ����������� ����� `SubscribeAsync<T>`:

```csharp
// �������� � �������������� �������������� ���������
await _rabbitMqClient.SubscribeAsync<OrderCreatedMessage>(
    queueName: "orders.created",
    onMessageReceived: async (message, context) =>
    {
        Console.WriteLine($"������� ����� #{message.OrderId} �� ����� {message.Amount}");
        // ��������� ���������...
    },
    autoAck: true
);

// �������� � ������ �������������� ���������
await _rabbitMqClient.SubscribeAsync<OrderCreatedMessage>(
    queueName: "orders.created",
    onMessageReceived: async (message, context) =>
    {
        try
        {
            Console.WriteLine($"��������� ������ #{message.OrderId}");
            // ��������� ���������...
            
            // ������������� �������� ���������
            await context.AckAsync();
        }
        catch (Exception ex)
        {
            // ���������� ��������� � ��������� ����������� � �������
            await context.NackAsync(requeue: true);
        }
    },
    autoAck: false
);
```

### ���������� ��������������� ���������

��� ������� ���������� ��������������� ��������� ����������� ������ `AckAsync()` � `NackAsync()` �� ������� `MessageContext`:

```csharp
await _rabbitMqClient.SubscribeAsync<MyMessage>(
    queueName: "my.queue",
    onMessageReceived: async (message, context) =>
    {
        try
        {
            // �������� ���������
            await context.AckAsync();
        }
        catch (Exception)
        {
            // ��������� ���������, ��������� �����
            await context.NackAsync(requeue: true);
            
            // ��� ��������� � Dead Letter Queue
            // await context.NackAsync(requeue: false);
        }
    },
    autoAck: false
);
```

## �������

### ������� ������
```csharp
// ������� ������ ���������� � ��������

// 1. ��������� ��������
services.AddRabbitMq(options => 
{
    options.Host = "localhost";
    options.Username = "guest";
    options.Password = "guest";
});

// 2. ������������� � �������
public class NotificationService
{
    private readonly IRabbitMqClient _rabbitMqClient;

    public NotificationService(IRabbitMqClient rabbitMqClient)
    {
        _rabbitMqClient = rabbitMqClient;
    }

    public async Task SendNotificationAsync(string userId, string message)
    {
        await _rabbitMqClient.PublishAsync("notifications", new
        {
            UserId = userId,
            Message = message,
            Timestamp = DateTime.UtcNow
        });
    }

    public async Task StartNotificationProcessingAsync()
    {
        await _rabbitMqClient.SubscribeAsync<dynamic>(
            "notifications",
            async (notification, context) =>
            {
                Console.WriteLine($"�������� ����������� ������������ {notification.UserId}: {notification.Message}");
                // ������ �������� �����������...
            }
        );
    }
}
```

### ����������� ������

```csharp
// ����� ������� ������ � ���������� ������ � ������ ��������������

// ����� ���������
public class PaymentProcessedMessage
{
    public string TransactionId { get; set; }
    public string CustomerId { get; set; }
    public decimal Amount { get; set; }
    public bool Success { get; set; }
    public string ErrorMessage { get; set; }
}

// ������ ��������� ��������
public class PaymentService
{
    private readonly IRabbitMqClient _rabbitMqClient;
    private readonly ILogger<PaymentService> _logger;

    public PaymentService(IRabbitMqClient rabbitMqClient, ILogger<PaymentService> logger)
    {
        _rabbitMqClient = rabbitMqClient;
        _logger = logger;
    }

    public async Task ProcessPaymentAsync(string customerId, decimal amount)
    {
        var transactionId = Guid.NewGuid().ToString();
        
        try
        {
            // ������ ��������� �������...
            bool paymentSuccessful = await ProcessPaymentLogic(customerId, amount);
            
            // ���������� ����������
            await _rabbitMqClient.PublishAsync("payments.processed", new PaymentProcessedMessage
            {
                TransactionId = transactionId,
                CustomerId = customerId,
                Amount = amount,
                Success = paymentSuccessful,
                ErrorMessage = paymentSuccessful ? null : "Insufficient funds"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "������ ��� ��������� �������");
            
            // ���������� ��������� �� ������
            await _rabbitMqClient.PublishAsync("payments.processed", new PaymentProcessedMessage
            {
                TransactionId = transactionId,
                CustomerId = customerId,
                Amount = amount,
                Success = false,
                ErrorMessage = ex.Message
            });
        }
    }

    public async Task StartPaymentNotificationServiceAsync()
    {
        await _rabbitMqClient.SubscribeAsync<PaymentProcessedMessage>(
            "payments.processed",
            async (payment, context) =>
            {
                try
                {
                    if (payment.Success)
                    {
                        _logger.LogInformation(
                            "�������� ������: ���������� {TransactionId}, ������ {CustomerId}, ����� {Amount}",
                            payment.TransactionId, payment.CustomerId, payment.Amount);
                        
                        // �������� ����������� ������� �� �������� �������
                        await SendSuccessNotification(payment);
                    }
                    else
                    {
                        _logger.LogWarning(
                            "��������� ������: ���������� {TransactionId}, ������ {CustomerId}, ������: {Error}",
                            payment.TransactionId, payment.CustomerId, payment.ErrorMessage);
                        
                        // �������� ����������� ������� � ��������� �������
                        await SendFailureNotification(payment);
                    }
                    
                    // ������������� ��������� ���������
                    await context.AckAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "������ ��� ��������� ����������� � �������");
                    
                    // ������� ��������� � ������� ��� ��������� ���������
                    await context.NackAsync(requeue: true);
                }
            },
            autoAck: false
        );
    }

    // ��������������� ������...
    private Task<bool> ProcessPaymentLogic(string customerId, decimal amount) => Task.FromResult(true);
    private Task SendSuccessNotification(PaymentProcessedMessage payment) => Task.CompletedTask;
    private Task SendFailureNotification(PaymentProcessedMessage payment) => Task.CompletedTask;
}
```

## ����������

- .NET 9.0 ��� ����
- RabbitMQ Server 3.8.0 ��� ����

## ��������

���� ������ ������������ ��� [MIT License](LICENSE).