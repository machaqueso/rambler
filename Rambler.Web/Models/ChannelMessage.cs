namespace Rambler.Web.Models
{
    /// <summary>
    /// Message sent to a "channel" through signalr
    /// </summary>
    public class ChannelMessage
    {
        public string Channel { get; set; }
        public ChatMessage ChatMessage { get; set; }
    }
}
