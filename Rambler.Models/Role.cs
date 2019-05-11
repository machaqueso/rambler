namespace Rambler.Models
{
    public class Role
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        public virtual User User { get; set; }
    }
}