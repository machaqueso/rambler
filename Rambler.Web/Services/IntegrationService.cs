using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Rambler.Web.Data;
using Rambler.Models;

namespace Rambler.Web.Services
{
    public class IntegrationService
    {
        private readonly DataContext db;
        private readonly IntegrationManager integrationManager;

        public IntegrationService(DataContext db, IntegrationManager integrationManager)
        {
            this.db = db;
            this.integrationManager = integrationManager;
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

            integrationManager.IntegrationEvent(integration.Name, integration.IsEnabled);
        }

        public async Task<bool> IsEnabled(string name)
        {
            var integration = await GetIntegrations()
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Name == name);

            if (integration == null)
            {
                return false;
            }

            return integration.IsEnabled;
        }

    }
}