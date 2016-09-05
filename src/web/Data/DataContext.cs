using Microsoft.EntityFrameworkCore;
using Rambler.Models;

public class DataContext : DbContext
{
    public DbSet<GoogleToken> GoogleTokens { get; set; }
    public DbSet<User> Users {get;set;}

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder){
        optionsBuilder.UseSqlite("Filename=./data.db");
    }
}
