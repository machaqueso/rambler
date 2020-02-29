using Rambler.Data;

namespace Rambler.Test
{
    public static class Utilities
    {
        
        public static void InitializeDbForTests(DataContext db)
        {
            //db.Messages.AddRange(GetSeedingMessages());
            db.SaveChanges();
        }

        public static void ReinitializeDbForTests(DataContext db)
        {
            db.Messages.RemoveRange(db.Messages);
            InitializeDbForTests(db);
        }

    }
}