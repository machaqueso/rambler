using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Rambler.Data;
using Rambler.Models;

namespace Rambler.Web.Services
{
    public class ChannelService
    {
        private readonly DataContext db;

        public ChannelService(DataContext db)
        {
            this.db = db;
        }

        public IQueryable<Channel> GetChannels()
        {
            return db.Channels;
        }

        public async Task UpdateChannel(int id, Channel channel)
        {
            var record = await GetChannels().FirstOrDefaultAsync(x => x.Id == id);

            if (record == null)
            {
                throw new InvalidOperationException($"Channel id '{id}' not found.");
            }

            db.Entry(record).CurrentValues.SetValues(channel);
            await db.SaveChangesAsync();
        }

    }
}