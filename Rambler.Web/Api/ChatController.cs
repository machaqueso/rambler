using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Rambler.Web.Models;
using Rambler.Web.Services;

namespace Rambler.Web.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChatController : ControllerBase
    {
        private readonly ChatService chatService;

        public ChatController(ChatService chatService)
        {
            this.chatService = chatService;
        }

        [Route("")]
        public IActionResult GetMessages()
        {
            var messages = chatService.GetMessages()
                .OrderByDescending(x => x.Date)
                .Take(10)
                .OrderBy(x => x.Date);

            return Ok(messages.Select(x => new
            {
                x.Id,
                x.Date,
                x.Author,
                x.Source,
                x.SourceAuthorId,
                x.SourceMessageId,
                x.Message,
                DisplayDate = DateTime.Now.DayOfYear != x.Date.ToLocalTime().DayOfYear ? x.Date.ToLocalTime().ToString("d") : "",
                DisplayTime = x.Date.ToLocalTime().ToString("t")
            }));
        }

        [Route("{id}")]
        public IActionResult GetMessage(int id)
        {
            var message = chatService.GetMessages()
                .FirstOrDefault(x => x.Id == id);

            if (message == null)
            {
                return NotFound();
            }

            return Ok(message);
        }

        [Route("")]
        [HttpPost]
        public async Task<IActionResult> CreateMessage(ChatMessage message)
        {
            if (message == null)
            {
                return BadRequest();
            }

            await chatService.CreateMessage(message);
            return CreatedAtRoute("GetMessage", new { id = message.Id }, message);
        }

    }
}