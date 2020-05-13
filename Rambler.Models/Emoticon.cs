namespace Rambler.Models
{
    public class Emoticon
    {
        public int Id { get; set; }
        public string SourceId { get; set; }
        public string Regex { get; set; }
        public string Url { get; set; }
        public string ApiSource { get; set; }
        public int? ChannelId { get; set; }
        public virtual Channel Channel { get; set; }
    }
}