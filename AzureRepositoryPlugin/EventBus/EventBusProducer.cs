using MassTransit;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;
using MassTransit.ActiveMqTransport;
using MassTransit.AmazonSqsTransport.Configuration;
using MassTransit.Azure.ServiceBus.Core;
using Microsoft.Extensions.Configuration;
using AzureRepositoryPlugin;

namespace AzureRepositoryPlugin.EventBus
{
    public class EventBusProducer : EventBus
    {
        public EventBusProducer() : base()
        {
            Type = "producer";

            switch (ServiceName)
            {
                case RabbitMq:
                    BusControl = Bus.Factory.CreateUsingRabbitMq(cfg =>
                    {
                        IHost host = GetHost(cfg);
                    });
                    break;
                case AmazonSqs:
                    BusControl = Bus.Factory.CreateUsingAmazonSqs(cfg =>
                    {
                        IHost host = GetHost(cfg);
                    });
                    break;
                case AzureServiceBus:
                    BusControl = Bus.Factory.CreateUsingAzureServiceBus(cfg =>
                    {
                        IHost host = GetHost(cfg);
                    });
                    break;
                case ActiveMq:
                    BusControl = Bus.Factory.CreateUsingActiveMq(cfg =>
                    {
                        IHost host = GetHost(cfg);
                    });
                    break;
                default:
                    BusControl = Bus.Factory.CreateUsingInMemory(cfg =>
                    {
                        IHost host = GetHost(cfg);
                    });
                    break;
            }
        }

        public async void Publish(JobRequestData evt)
        {
            await BusControl.Publish(evt);
        }
    }
}
