using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace Rambler.Models
{
    public class Integration
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public bool IsEnabled { get; set; }
        public string Status { get; set; }
        public DateTime? UpdateDate { get; set; }
        public bool IsVisible { get; set; }
        public int DisplayOrder { get; set; }

        [NotMapped] public string DisplayEnabled => IsEnabled ? "Enabled" : "Disabled";

        [NotMapped]
        public string[] InactiveStatuses =
        {
            IntegrationStatus.Stopped,
            IntegrationStatus.Stopping,
            IntegrationStatus.Error,
            IntegrationStatus.Forbidden
        };

        [NotMapped]
        public bool IsInactive => InactiveStatuses.Contains(Status);
    }
}