using Rambler.Data;
using Rambler.Services;
using System;
using System.Linq;

namespace Rambler.Test
{
    public static class Utilities
    {
        
        public static void InitializeDbForTests(DataContext db)
        {
            //db.Messages.AddRange(GetSeedingMessages());

            var admin = db.Users.SingleOrDefault(x => x.UserName == "admin");
            if (admin != null)
            {
                var passwordService = new PasswordService();
                admin.PasswordHash = Convert.ToBase64String(passwordService.HashPasswordV3("password"));
                admin.MustChangePassword = false;
            }

            db.SaveChanges();
        }

        public static void ReinitializeDbForTests(DataContext db)
        {
            db.Messages.RemoveRange(db.Messages);
            InitializeDbForTests(db);
        }

    }
}