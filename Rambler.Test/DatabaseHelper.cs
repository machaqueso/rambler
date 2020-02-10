using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Rambler.Data;

namespace Rambler.Test
{
    public class DatabaseHelper
    {

        public static void Init(IServiceScope scope)
        {
            var db = scope.ServiceProvider.GetService<DataContext>();
            db?.Database.EnsureDeleted();
            db?.Database.Migrate();
        }

    }
}