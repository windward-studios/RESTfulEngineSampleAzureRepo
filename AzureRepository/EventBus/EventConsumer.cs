using MassTransit;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AzureRepositoryPlugin.EventBus
{
    public class EventConsumer: IConsumer<JobRequestData>
    {
        public delegate Task MessageConsumedEventHandler(object sender, EventArgs<JobRequestData> e);
        public static event MessageConsumedEventHandler OnMessageConsumed;

        public static void AddToEndpoint(IReceiveEndpointConfigurator ep)
        {
            ep.Consumer<EventConsumer>();
        }

        public async Task Consume(ConsumeContext<JobRequestData> context)
        {
            var args = new EventArgs<JobRequestData>(context.Message);
            MessageConsumedEventHandler handler = OnMessageConsumed;
            if (handler != null)
                await handler(this, args);
        }
    }
}
