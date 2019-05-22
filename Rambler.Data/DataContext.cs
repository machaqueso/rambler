using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Rambler.Models;

namespace Rambler.Data
{
    public class DataContext : DbContext
    {
        private readonly IConfiguration configuration;

        public DataContext(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite(configuration.GetConnectionString("DefaultConnection"));
        }

        public DbSet<User> Users { get; set; }
        public DbSet<ChatMessage> Messages { get; set; }
        public DbSet<AccessToken> AccessTokens { get; set; }
        public DbSet<ConfigurationSetting> ConfigurationSettings { get; set; }
        public DbSet<Integration> Integrations { get; set; }
        public DbSet<Channel> Channels { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.Entity<ConfigurationSetting>().HasData(
                new ConfigurationSetting
                {
                    Id = 1,
                    Name = "Youtube client id",
                    Key = "Authentication:Google:ClientId"
                },
                new ConfigurationSetting
                {
                    Id = 2,
                    Name = "Youtube client secret",
                    Key = "Authentication:Google:ClientSecret"
                },
                new ConfigurationSetting
                {
                    Id = 3,
                    Name = "Twitch client id",
                    Key = "Authentication:Twitch:ClientId"
                },
                new ConfigurationSetting
                {
                    Id = 4,
                    Name = "Twitch client secret",
                    Key = "Authentication:Twitch:ClientSecret"
                }
            );

            builder.Entity<Integration>().HasData(
                new Integration
                {
                    Id = 1,
                    Name = "Youtube",
                    IsEnabled = false
                },
                new Integration
                {
                    Id = 2,
                    Name = "Twitch",
                    IsEnabled = false
                }
            );

            builder.Entity<Channel>().HasData(
                new Channel
                {
                    Id = 1,
                    Name = "All"
                },
                new Channel
                {
                    Id = 2,
                    Name = "Reader"
                },
                new Channel
                {
                    Id = 3,
                    Name = "OBS"
                },
                new Channel
                {
                    Id = 4,
                    Name = "TTS"
                }
            );

            builder.Entity<User>().HasData(
                new User()
                {
                    Id = 1,
                    UserName = "Admin",
                    MustChangePassword = true
                }
            );
        }

    }

    // This is needed for entity framework migrations to work when configuration is passed to datacontext
    public class DataContextDbFactory : IDesignTimeDbContextFactory<DataContext>
    {
        DataContext IDesignTimeDbContextFactory<DataContext>.CreateDbContext(string[] args)
        {
            IConfigurationRoot configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();

            return new DataContext(configuration);
        }
    }
}