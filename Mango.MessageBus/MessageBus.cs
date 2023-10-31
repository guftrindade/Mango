using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mango.MessageBus;

public class MessageBus : IMessageBus
{
    public Task PublishMessage(object message, string topic_queue_Name)
    {
        throw new NotImplementedException();
    }
}
