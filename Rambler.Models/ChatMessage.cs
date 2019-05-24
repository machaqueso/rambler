using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Rambler.Models
{
    public class ChatMessage
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public string Message { get; set; }
        public string Source { get; set; }
        public string SourceMessageId { get; set; }

        public int AuthorId { get; set; }
        public virtual Author Author { get; set; }

        [NotMapped] public string DisplayDate => DateTime.Now.DayOfYear != Date.ToLocalTime().DayOfYear ? Date.ToLocalTime().ToString("d") : "";
        [NotMapped] public string DisplayTime => Date.ToLocalTime().ToString("t");
    }

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

    public class AuthorScoreHistory
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public int Value { get; set; }
        public string Reason { get; set; }

        public int AuthorId { get; set; }
        public virtual Author Author { get; set; }
    }

    public class AuthorFilter
    {
        public int Id { get; set; }
        public string FilterType { get; set; }
        public DateTime Date { get; set; }

        public int AuthorId { get; set; }
        public virtual Author Author { get; set; }
    }

    public class FilterTypes
    {
        public static string Whitelist = "Whitelist";
        public static string Ignorelist = "Ignorelist";
        public static string Blacklist = "Blacklist";
        public static string Banlist = "Banlist";
    }
}