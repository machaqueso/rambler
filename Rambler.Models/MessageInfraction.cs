namespace Rambler.Models
{
    public class MessageInfraction
    {
        public int Id { get; set; }
        public int MessageId { get; set; }
        public virtual ChatMessage Message { get; set; }
        public string InfractionType { get; set; }
        public string Description { get; set; }
    }
}