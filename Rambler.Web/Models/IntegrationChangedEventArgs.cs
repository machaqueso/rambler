using System;

namespace Rambler.Web.Models
{
    public class IntegrationChangedEventArgs : EventArgs
    {
        public IntegrationChangedEventArgs(bool isEnabled)
        {
            IsEnabled = isEnabled;
        }

        public bool IsEnabled { get; set; }
    }
}