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

        public IntegrationService(DataContext db)
        {
            this.db = db;
        }

        public event EventHandler<IntegrationChangedEventArgs> IntegrationChanged;

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
            var integration = await GetIntegrations()
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Name == name);

            if (integration == null)
            {
                return false;
            }



            return integration.IsEnabled;
        }

        public void IntegrationEvent(bool isEnabled)
        {
            var handler = IntegrationChanged;
            if (handler != null)
            {
                var args = new IntegrationChangedEventArgs(isEnabled);
                handler(this, args);
            }
        }

    }
}