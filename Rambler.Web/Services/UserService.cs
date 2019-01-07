using System.Linq;
using Rambler.Web.Data;
using Rambler.Web.Models;

namespace Rambler.Web.Services
{
    public class UserService
    {
        private readonly DataContext db;

        public UserService(DataContext db)
        {
            this.db = db;
        }

        public IQueryable<User> GetUsers()
        {
            return db.Users;
        }
    }
}