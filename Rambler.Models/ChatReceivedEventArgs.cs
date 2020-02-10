using System;

namespace Rambler.Models
{
    public class ChatReceivedEventArgs : EventArgs
    {
        public ChatReceivedEventArgs(ChatMessage chatMessage)
        {
            ChatMessage = chatMessage;
        }

        public ChatMessage ChatMessage { get; set; }
    }
}