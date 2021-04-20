using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text;
using System.Threading.Tasks;
using AzureRepositoryPlugin;
using MassTransit;
using MassTransit.ActiveMqTransport;
using MassTransit.AmazonSqsTransport.Configuration;
using MassTransit.Azure.ServiceBus.Core;
using MassTransit.Testing;
using Microsoft.Extensions.Configuration;

namespace AzureRepositoryPlugin.EventBus
{
    public class EventBusConsumer : EventBus
    {
        public EventBusConsumer() 
            : this(ConfigurationManager.AppSettings["BusService:ConsumerQueueName"])
        {
        }

        public EventBusConsumer(string queueName): base()
        {
            QueueName = queueName;
            Type = "consumer";
        }

        public void Create(Action<IReceiveEndpointConfigurator> consumers)
        {
            switch (ServiceName)
            {
                case RabbitMq:
                    BusControl = Bus.Factory.CreateUsingRabbitMq(cfg =>
                    {
                        IHost host = GetHost(cfg);
                        AddEndpoints(cfg, consumers);
                    });
                    break;
                case AmazonSqs:
                    BusControl = Bus.Factory.CreateUsingAmazonSqs(cfg =>
                    {
                        IHost host = GetHost(cfg);
                        AddEndpoints(cfg, consumers);
                    });
                    break;
                case AzureServiceBus:
                    BusControl = Bus.Factory.CreateUsingAzureServiceBus(cfg =>
                    {
                        IHost host = GetHost(cfg);
                        AddEndpoints(cfg, consumers);
                    });
                    break;
                case ActiveMq:
                    BusControl = Bus.Factory.CreateUsingActiveMq(cfg =>
                    {
                        IHost host = GetHost(cfg);
                        AddEndpoints(cfg, consumers);
                    });
                    break;
                case UnitTest:
                default:
                    BusControl = Bus.Factory.CreateUsingInMemory(cfg =>
                    {
                        IHost host = GetHost(cfg);
                        AddEndpoints(cfg, consumers);
                    });
                    break;
            }
        }

        private void AddEndpoints(IBusFactoryConfigurator cfg, Action<IReceiveEndpointConfigurator> consumers)
        {
            cfg.ReceiveEndpoint(QueueName, ep =>
            {
                consumers.Invoke(ep);
            });
        }
    }
}
