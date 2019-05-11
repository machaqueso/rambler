namespace Rambler.Models
{
    public class ExternalAccount
    {
        public int Id { get; set; }
        public string ApiSource { get; set; }
        public string ReferenceId { get; set; }
        public string Username { get; set; }

        public virtual User User { get; set; }
    }
}