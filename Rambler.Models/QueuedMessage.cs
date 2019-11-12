namespace Rambler.Models
{
    public class QueuedMessage
    {
        public int Id { get; set; }
        public int MessageId { get; set; }
        public virtual ChatMessage Message { get; set; }
        public string IntegrationName { get; set; }
    }
}