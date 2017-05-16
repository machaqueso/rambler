using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Rambler.Models;

namespace Rambler.Web.Controllers
{
    [Route("api/[controller]")]
    public class ChatController : Controller
    {
        private readonly DataContext db;

        public ChatController(DataContext dataContext)
        {
            db = dataContext;
        }

        [HttpGet]
        public IEnumerable<ChatMessage> Get()
        {
            return db.Messages;
        }

        [HttpGet("{id}")]
        public async Task<ChatMessage> Get(int id)
        {
            return await db.Messages.FindAsync(id);
        }

        [HttpPut("{id}")]
        public async void Put(int id, [FromBody] ChatMessage chatMessage)
        {
            db.Messages.Update(chatMessage);
            await db.SaveChangesAsync();
        }
    }
}