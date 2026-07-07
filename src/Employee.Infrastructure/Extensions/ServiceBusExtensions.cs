using Azure.Messaging.ServiceBus;
using Employee.Application.Common.Events;
using Employee.Application.Common.Messaging;
using Employee.Infrastructure.Messaging.ServiceBus;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Employee.Infrastructure.Extensions
{
    public static class ServiceBusExtensions
    {
        public static IServiceCollection AddAzureServiceBus(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            var serviceBusConnectionString = configuration.GetConnectionString("ServiceBus")
                ?? throw new InvalidOperationException("Service Bus connection string not found in configuration");

            // Register Azure Service Bus client
            services.AddSingleton(new ServiceBusClient(serviceBusConnectionString));

            // Register event publisher
            services.AddSingleton<IEventPublisher>(provider =>
            {
                var client = provider.GetRequiredService<ServiceBusClient>();
                var logger = provider.GetRequiredService<ILogger<ServiceBusEventPublisher>>();
                return new ServiceBusEventPublisher(client, logger, "employee-events");
            });

            // Register event subscriber as hosted service
            services.AddHostedService<ServiceBusEventSubscriber>();

            return services;
        }

        /// <summary>
        /// Configure specific event handlers for the Service Bus subscriber
        /// </summary>
        public static IServiceCollection AddServiceBusEventHandlers(
            this IServiceCollection services)
        {
            // Register event handlers here if needed
            // These can be called from IEventSubscriber
            
            return services;
        }
    }
}
