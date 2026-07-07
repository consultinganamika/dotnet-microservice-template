using Azure.Messaging.ServiceBus;
using Employee.Application.Common.Events;
using Employee.Application.Common.Messaging;
using Employee.Application.Extensions;
using Employee.Infrastructure.Extensions;
using Employee.Infrastructure.Messaging;
using Employee.Infrastructure.Messaging.Outbox;
using Employee.Infrastructure.Repositories;

namespace Employee.API.Extensions
{
    public static class MessagingExtensions
    {
        /// <summary>
        /// Add messaging services including Service Bus and Outbox Pattern
        /// </summary>
        public static IServiceCollection AddMessagingServices(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            // Add Azure Service Bus
            services.AddAzureServiceBus(configuration);

            // Add Outbox Repository
            services.AddScoped<IOutboxRepository, OutboxRepository>();

            // Add background service for processing outbox messages
            services.AddHostedService<OutboxPublisherBackgroundService>();

            return services;
        }
    }
}
