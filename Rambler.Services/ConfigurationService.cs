using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Rambler.Data;
using Rambler.Models;
using Rambler.Models.Exceptions;

namespace Rambler.Services
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

        public async Task CreateSetting(ConfigurationSetting setting)
        {
            if (GetSettings().Any(x => x.Name == setting.Name))
            {
                throw new ConflictException($"Setting {setting.Name} already exists");
            }

            await db.ConfigurationSettings.AddAsync(setting);
            await db.SaveChangesAsync();
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

        public async Task DeleteSetting(int id)
        {
            var entity = GetSettings().FirstOrDefault(x => x.Id == id);
            if (entity == null)
            {
                throw new InvalidOperationException("Not found");
            }

            db.ConfigurationSettings.Remove(entity);
            await db.SaveChangesAsync();
        }

        public IEnumerable<string> GetConfigurationSettingNames()
        {
            return ConfigurationSettingNames.All;
        }
    }
}