using MassTransit;
using MassTransit.ActiveMqTransport;
using MassTransit.AmazonSqsTransport;
using MassTransit.AmazonSqsTransport.Configuration;
using MassTransit.Azure.ServiceBus.Core;
using MassTransit.RabbitMqTransport;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;

namespace AzureRepositoryPlugin.EventBus
{
    public class EventBus
    {
        public string QueueName { get; set; }

        public string ServiceName { get; set; }

        public string Type { get; set; }

        public IHost Host { get; set; }

        public IBusControl BusControl { get; set; }

        protected const string RabbitMq = "RabbitMQ";
        protected const string AzureServiceBus = "AzureServiceBus";
        protected const string AmazonSqs = "AmazonSQS";
        protected const string ActiveMq = "ActiveMQ";
        protected const string UnitTest = "UnitTest"; // Intended for single-process unit testing only

        private static readonly Dictionary<string, string[]> BusServiceConfigurationValues = new Dictionary<string, string[]>()
        {
            {UnitTest, new string[]{"Uri"}},
            {RabbitMq, new string[]{"Uri", "Username", "Password"}},
            {AzureServiceBus, new string[]{"Uri", "KeyName", "SharedAccessKey"}},
            {AmazonSqs, new string[]{"Region", "AccessKey", "SecretKey"}},
            {ActiveMq, new string[]{"Uri", "Username", "Password"}}
        };

        public EventBus()
        {
            ServiceName = ConfigurationManager.AppSettings["BusService:Name"];
            if (!IsValidServiceName(ServiceName))
            {
                throw new ArgumentException("Invalid Bus Service Type", ServiceName);
            }
        }

        public async void Start()
        {
            await BusControl.StartAsync();
        }

        public async void Stop()
        {
            await BusControl.StopAsync();
        }

        public IHost GetHost(IBusFactoryConfigurator cfg)
        {
            if (ServiceName.Equals(RabbitMq))
            {
                return ((IRabbitMqBusFactoryConfigurator)cfg).Host(new Uri(Get("Uri")), host =>
                {
                    host.Username(Get("Username"));
                    host.Password(Get("Password"));
                });
            }
            if (ServiceName.Equals(AzureServiceBus))
            {
                return ((IServiceBusBusFactoryConfigurator)cfg).Host(new Uri(Get("Uri")), host =>
                {
                    host.SharedAccessSignature(s =>
                    {
                        s.KeyName = Get("KeyName");
                        s.SharedAccessKey = Get("SharedAccessKey");
                    });
                });
            }
            if (ServiceName.Equals(AmazonSqs))
            {
                string region = Get("Region");
                string accessKey = Get("AccessKey");
                string secretKey = Get("SecretKey");

                return ((IAmazonSqsBusFactoryConfigurator)cfg).Host(region, h =>
                {
                    h.AccessKey(accessKey);
                    h.SecretKey(secretKey);
                });
            }
            if (ServiceName.Equals(ActiveMq))
            {
                return ((IActiveMqBusFactoryConfigurator)cfg).Host(new Uri(Get("Uri")), host =>
                {
                    host.Username(Get("Username"));
                    host.Password(Get("Password"));

                    // Should this be configurable?
                    host.UseSsl();
                });
            }
            if (ServiceName.Equals(UnitTest))
            {
                return ((IInMemoryBusFactoryConfigurator)cfg).Host();
            }

            return ((IInMemoryBusFactoryConfigurator)cfg).Host();
        }

        private bool IsValidServiceName(string serviceName)
        {
            return serviceName.Equals(UnitTest) ||
                   serviceName.Equals(RabbitMq) ||
                   serviceName.Equals(AzureServiceBus) ||
                   serviceName.Equals(AmazonSqs) ||
                   serviceName.Equals(ActiveMq);
        }

        private string Get(string key)
        {
            if (BusServiceConfigurationValues.TryGetValue(ServiceName, out var validKeys))
            {
                if (validKeys.Contains(key))
                {
                    return ConfigurationManager.AppSettings[$"BusService:{key}"];
                    //return Configuration.GetSection("BusService:" + key).Value;
                }

                throw new ArgumentException("Key not valid for Service Bus Type " + ServiceName, key);
            }
            // If this would have returned false it would've been caught by the constructor 
            // and thrown an exception but this needs to be this way 
            return null;
        }
    }
}
