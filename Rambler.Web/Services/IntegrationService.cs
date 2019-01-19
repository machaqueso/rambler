using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Rambler.Web.Data;
using Rambler.Web.Models;

namespace Rambler.Web.Services
{
    public class IntegrationService
    {
        private readonly DataContext db;
        private readonly YoutubeBackgroundService youtube;
        private readonly TwitchBackgroundService twitch;

        public IntegrationService(DataContext db, YoutubeBackgroundService youtube, TwitchBackgroundService twitch)
        {
            this.db = db;
            this.youtube = youtube;
            this.twitch = twitch;
        }

        public IQueryable<Integration> GetIntegrations()
        {
            return db.Integrations;
        }

        public async Task UpdateIntegration(int id, Integration integration)
        {
            var record = await GetIntegrations().FirstOrDefaultAsync(x => x.Id == id);

            if (record == null)
            {
                throw new InvalidOperationException($"Integration id '{id}' not found.");
            }

            db.Entry(record).CurrentValues.SetValues(integration);
            await db.SaveChangesAsync();
        }

        public async Task<bool> IsEnabled(string name)
        {
            var integration = await GetIntegrations().FirstOrDefaultAsync(x => x.Name == name);
            if (integration == null)
            {
                return false;
            }

            return integration.IsEnabled;
        }

    }
}