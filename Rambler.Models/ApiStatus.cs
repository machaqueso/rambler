using System;

namespace Rambler.Models
{
    public class ApiStatus
    {
        public ApiStatus() { }

        public ApiStatus(string name)
        {
            Name = name;
            Status = BackgroundServiceStatus.Stopped;
            UpdateDate = DateTime.UtcNow;
        }

        public ApiStatus(string name, string status)
        {
            Name = name;
            Status = status;
            UpdateDate = DateTime.UtcNow;
        }

        public string Name { get; set; }
        public string Status { get; set; }
        public DateTime UpdateDate { get; set; }
    }
}