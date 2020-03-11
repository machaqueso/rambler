using System;
using Rambler.Models;

namespace Rambler.Services
{
    public class IntegrationManager
    {
        public event EventHandler<IntegrationChangedEventArgs> IntegrationChanged;
        public event EventHandler<MessageSentEventArgs> MessageSent;

        public void IntegrationEvent(string name, bool isEnabled)
        {
            var handler = IntegrationChanged;
            if (handler != null)
            {
                var args = new IntegrationChangedEventArgs(name, isEnabled);
                handler(this, args);
            }
        }

        public void MessageSentEvent(string message)
        {
            var handler = MessageSent;
            if (handler != null)
            {
                var args = new MessageSentEventArgs(message);
                handler(this, args);
            }
        }

    }
}
