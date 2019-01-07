using Microsoft.EntityFrameworkCore;
using Rambler.Web.Models;

namespace Rambler.Web.Data
{
    public class DataContext : DbContext
    {
        public DbSet<GoogleToken> GoogleTokens { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<ChatMessage> Messages { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Filename=./data.db");
        }
    }
}