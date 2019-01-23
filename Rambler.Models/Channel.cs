namespace Rambler.Models
{
    public class Channel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int? MinimumReputation { get; set; }
        public int? MaximumReputation { get; set; }
    }
}