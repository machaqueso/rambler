using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Rambler.Models;
using Rambler.Models.Twitch;

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
        public DbSet<TwitchUser> TwitchUsers { get; set; }
        public DbSet<Author> Authors { get; set; }
        public DbSet<AuthorScoreHistory> AuthorScoreHistories { get; set; }
        public DbSet<AuthorFilter> AuthorFilters { get; set; }
        public DbSet<WordFilter> WordFilters { get; set; }
        public DbSet<BotAction> BotActions { get; set; }
        public DbSet<MessageInfraction> MessageInfractions { get; set; }
        public DbSet<Log> Logs { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Log>(entity => { entity.ToView("Logs"); });

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
                    UserName = "admin",
                    MustChangePassword = true
                }
            );
        }

        private Exception HandleDbUpdateException(DbUpdateException dbu)
        {
            var builder = new StringBuilder("A DbUpdateException was caught while saving changes. ");

            try
            {
                foreach (var result in dbu.Entries)
                {
                    builder.AppendFormat("Type: {0} was part of the problem. ", result.Entity.GetType().Name);
                }
            }
            catch (Exception e)
            {
                builder.Append("Error parsing DbUpdateException: " + e.ToString());
            }

            string message = builder.ToString();
            return new Exception(message, dbu);
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                return await base.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateException dbex)
            {
                var ex = HandleDbUpdateException(dbex);
                throw ex;
            }
        }

    }
}