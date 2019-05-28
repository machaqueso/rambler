using System;

namespace Rambler.Models
{
    public class AuthorScoreHistory
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public int Value { get; set; }
        public string Reason { get; set; }

        public int AuthorId { get; set; }
        public virtual Author Author { get; set; }
    }
}