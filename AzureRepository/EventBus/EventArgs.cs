using MassTransit;
using System;
using System.Collections.Generic;
using System.Text;

namespace AzureRepositoryPlugin.EventBus
{
    public class EventArgs<T> : EventArgs where T : JobRequestData
    {
        public T Message { get; set; }

        public Fault<T> Fault { get; set; }

        public EventArgs(T e)
        {
            Message = e;
        }

        public EventArgs(Fault<T> fault)
        {
            Fault = fault;
            Message = fault.Message;
        }
    }
}
