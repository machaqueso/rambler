using System.Collections.Generic;

namespace Rambler.Models
{
    public class Author
    {
        public int Id { get; set; }
        public string Source { get; set; }
        public string SourceAuthorId { get; set; }
        public string Name { get; set; }
        public int Points { get; set; }
        public int Score { get; set; }

        public ICollection<ChatMessage> ChatMessages { get; set; }
        public ICollection<AuthorScoreHistory> AuthorScoreHistories { get; set; }
        public ICollection<AuthorFilter> AuthorFilters { get; set; }
    }
}