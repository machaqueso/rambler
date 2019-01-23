using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Rambler.Web.Data;
using Rambler.Models;

namespace Rambler.Web.Services
{
    public class ConfigurationService
    {
        private readonly DataContext db;
        private readonly IConfiguration configuration;

        public ConfigurationService(DataContext db, IConfiguration configuration)
        {
            this.db = db;
            this.configuration = configuration;
        }

        public async Task<string> GetValue(string key)
        {
            var value = configuration[key];

            // database setting overrides config/environment
            var dbValue = await db.ConfigurationSettings.FirstOrDefaultAsync(x => x.Key == key && !string.IsNullOrEmpty(x.Value));
            if (dbValue != null)
            {
                value = dbValue.Value;
            }

            return value;
        }

        public async Task SetValue(string key, string value)
        {
            var dbValue = await db.ConfigurationSettings.FirstOrDefaultAsync(x => x.Key == key);
            if (dbValue == null)
            {
                db.ConfigurationSettings.Add(new ConfigurationSetting
                {
                    Key = key,
                    Value = value
                });
                await db.SaveChangesAsync();
                return;
            }

            dbValue.Value = value;
            await db.SaveChangesAsync();
        }

        public bool HasValue(string key)
        {
            return !string.IsNullOrEmpty(configuration[key]) ||
                   db.ConfigurationSettings.Any(x => x.Key == key && !string.IsNullOrEmpty(x.Value));
        }

        public IEnumerable<ConfigurationSetting> GetSettings()
        {
            var settings = db.ConfigurationSettings.ToList();

            foreach (var setting in settings.Where(x => string.IsNullOrEmpty(x.Value)).ToList())
            {
                setting.Value = configuration[setting.Key];
            }

            return settings;
        }

        public async Task UpdateSetting(int id, ConfigurationSetting setting)
        {
            var entity = GetSettings().FirstOrDefault(x => x.Id == id);
            if (entity == null)
            {
                throw new InvalidOperationException("Not found");
            }
            db.Entry(entity).CurrentValues.SetValues(setting);
            await db.SaveChangesAsync();
        }
    }
}