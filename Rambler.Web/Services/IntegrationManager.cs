using System;
using Rambler.Models;

namespace Rambler.Web.Services
{
    public class IntegrationManager
    {
        public event EventHandler<IntegrationChangedEventArgs> IntegrationChanged;

        public void IntegrationEvent(string name, bool isEnabled)
        {
            var handler = IntegrationChanged;
            if (handler != null)
            {
                var args = new IntegrationChangedEventArgs(name, isEnabled);
                handler(this, args);
            }
        }

    }
}