using System;

namespace Rambler.Web.Models
{
    public class ApiStatus
    {
        public ApiStatus() { }

        public ApiStatus(string name)
        {
            Name = name;
            Status = "Stopped";
            UpdateDate = DateTime.UtcNow;
        }

        public string Name { get; set; }
        public string Status { get; set; }
        public DateTime UpdateDate { get; set; }
    }
}