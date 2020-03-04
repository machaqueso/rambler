﻿using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Rambler.Data;

namespace Rambler.Test
{
    public class DatabaseHelper
    {

        public static void Init(IServiceScope scope)
        {
            var db = scope.ServiceProvider.GetService<DataContext>();

            if (!db.Database.GetService<IRelationalDatabaseCreator>().Exists())
            {
                db.Database.Migrate();
            }
        }

    }
}