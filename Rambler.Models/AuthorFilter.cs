using System;

namespace Rambler.Models
{
    public class AuthorFilter
    {
        public int Id { get; set; }
        public string FilterType { get; set; }
        public DateTime Date { get; set; }

        public int AuthorId { get; set; }
        public virtual Author Author { get; set; }
    }
}