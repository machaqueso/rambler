using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Rambler.Data;
using Rambler.Models;

namespace Rambler.Web.Services
{
    public class IntegrationService
    {
        private readonly DataContext db;
        private readonly IntegrationManager integrationManager;
        private readonly IEnumerable<IHostedService> backgroundServices;
        private readonly ILogger<IntegrationService> logger;


        public IntegrationService(DataContext db, IntegrationManager integrationManager, IEnumerable<IHostedService> backgroundServices, ILogger<IntegrationService> logger)
        {
            this.db = db;
            this.integrationManager = integrationManager;
            this.backgroundServices = backgroundServices;
            this.logger = logger;
        }

        public IQueryable<Integration> GetIntegrations()
        {
            return db.Integrations;
        }

        public async Task ToggleService(Integration integration)
        {
            logger.LogDebug($"background services found: {backgroundServices.Count()}");
            var service = backgroundServices.SingleOrDefault(x => x.GetType().Name.Contains(integration.Name));
            if (service == null)
            {
                logger.LogDebug($"{integration.Name} background service not found");
                return;
            }

            if (integration.IsEnabled)
            {
                await service.StartAsync(new CancellationToken());
                return;
            }
            await service.StopAsync(new CancellationToken());
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

            await ToggleService(integration);
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