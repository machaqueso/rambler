using System;

namespace Rambler.Web.Models
{
    public class IntegrationChangedEventArgs : EventArgs
    {
        public IntegrationChangedEventArgs(string name, bool isEnabled)
        {
            Name = name;
            IsEnabled = isEnabled;
        }

        public string Name { get; set; }
        public bool IsEnabled { get; set; }
    }
}