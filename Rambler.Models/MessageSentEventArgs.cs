using System;

namespace Rambler.Models
{
    public class MessageSentEventArgs : EventArgs
    {
        public MessageSentEventArgs(string message, string source)
        {
            Message = message;
            Source = source;
        }

        public string Message { get; set; }
        public string Source { get; set; }
    }
}
