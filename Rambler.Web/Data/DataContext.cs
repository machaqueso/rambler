using Microsoft.EntityFrameworkCore;
using Rambler.Web.Models;

namespace Rambler.Web.Data
{
    public class DataContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<ChatMessage> Messages { get; set; }
        public DbSet<AccessToken> AccessTokens { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Filename=./data.db");
        }
    }
}