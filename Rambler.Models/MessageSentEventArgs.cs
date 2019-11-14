using System;

namespace Rambler.Models
{
    public class MessageSentEventArgs : EventArgs
    {
        public MessageSentEventArgs(string message)
        {
            Message = message;
        }

        public string Message { get; set; }
    }
}
