using Microsoft.Extensions.DependencyInjection;
using System;
using paircalculator.Messaging;

namespace paircalculator.Extensions
{
    public static class RabbitMQServiceCollectionExtensions
    {
        public static IServiceCollection AddRabbitMQ(this IServiceCollection services)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            services.Add(ServiceDescriptor.Singleton<IMQClient, RabbitMQClient>());
            services.Add(ServiceDescriptor.Singleton<IMessageQueueSender, RabbitMQueueSender>());

            return services;
        }
    }
}