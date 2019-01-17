using System.ComponentModel.DataAnnotations.Schema;

namespace Rambler.Web.Models
{
    public class ConfigurationSetting
    {
        public int Id { get; set; }
        public string Key { get; set; }
        public string Value { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        [NotMapped] public bool Status => !string.IsNullOrEmpty(Value);
    }
}