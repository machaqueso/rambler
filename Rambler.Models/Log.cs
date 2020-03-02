namespace Rambler.Models
{
    public class Log
    {
        public int Id { get; set; }
        public string Timestamp { get; set; }
        public string Level{ get; set; }
        public string Exception { get; set; }
        public string RenderedMessage { get; set; }
        public string Properties { get; set; }
    }
}