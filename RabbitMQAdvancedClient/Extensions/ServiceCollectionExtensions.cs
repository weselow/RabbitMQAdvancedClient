using Microsoft.Extensions.DependencyInjection;
using RabbitMQAdvancedClient.Interfaces;
using RabbitMQAdvancedClient.Models;
using RabbitMQAdvancedClient.Services;

namespace RabbitMQAdvancedClient.Extensions
{
    /// <summary>
    /// Методы расширения для регистрации RabbitMQ библиотеки в DI.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Регистрирует RabbitMQ клиент и связанные сервисы в DI.
        /// </summary>
        /// <param name="services">Коллекция сервисов.</param>
        /// <param name="configureOptions">Делегат для настройки параметров подключения.</param>
        /// <returns>Коллекция сервисов.</returns>
        public static IServiceCollection AddRabbitMq(this IServiceCollection services, Action<RabbitMqOptions> configureOptions)
        {
            /* Пример использования:
            services.AddRabbitMq(options => {
                options.Host = "localhost";
                options.Port = 5672;
                options.Username = "guest";
                options.Password = "guest";
                options.RetryCount = 5;
                options.RetryDelay = 10;
            });
             *
             */
            // Настройка опций
            var options = new RabbitMqOptions();
            configureOptions(options);
            services.AddSingleton(options);

            // Регистрация сервисов
            services.AddSingleton<IConnectionManager, ConnectionManager>();
            services.AddSingleton<IRabbitMqClient, RabbitMqClient>();

            return services;
        }

        /// <summary>
        /// Регистрирует RabbitMQ клиент и связанные сервисы в DI с использованием предварительно настроенных параметров.
        /// </summary>
        /// <param name="services">Коллекция сервисов.</param>
        /// <param name="options">Предварительно настроенные параметры подключения.</param>
        /// <returns>Коллекция сервисов.</returns>
        public static IServiceCollection AddRabbitMq(this IServiceCollection services, RabbitMqOptions options)
        {
            // Регистрация параметров
            services.AddSingleton(options);

            // Регистрация сервисов
            services.AddSingleton<IConnectionManager, ConnectionManager>();
            services.AddSingleton<IRabbitMqClient, RabbitMqClient>();

            return services;
        }
    }
}