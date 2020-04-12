using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Rambler.Models;
using Rambler.Services;
using Rambler.Web.Services;

namespace Rambler.Web.Api
{
    [Route("api/[controller]")]
    [ApiController]
    [Area("api")]
    public class ChatController : ControllerBase
    {
        private readonly ChatMessageService chatMessageService;
        private readonly ChatProcessor chatProcessor;

        public ChatController(ChatMessageService chatMessageService, ChatProcessor chatProcessor)
        {
            this.chatMessageService = chatMessageService;
            this.chatProcessor = chatProcessor;
        }

        [Route("")]
        [HttpGet]
        [AllowAnonymous]
        public IActionResult GetMessages(int maxItems = 10)
        {
            var messages = chatMessageService.GetMessages()
                .OrderByDescending(x => x.Date)
                .Take(maxItems)
                .OrderBy(x => x.Date);

            return Ok(messages.Select(x => new
            {
                x.Id,
                x.Date,
                Author = x.Author.Name,
                AuthorId = x.Author.Id,
                x.Source,
                SourceAuthorId = x.Author.SourceAuthorId,
                x.SourceMessageId,
                x.Message,
                DisplayDate = DateTime.Now.DayOfYear != x.Date.ToLocalTime().DayOfYear ? x.Date.ToLocalTime().ToString("d") : "",
                DisplayTime = x.Date.ToLocalTime().ToString("t")
            }));
        }

        [Route("{id}", Name = "GetMessage")]
        [HttpGet]
        public IActionResult GetMessage(int id)
        {
            var message = chatMessageService.GetMessages()
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

            await chatProcessor.ProcessIncomingMessage(message);
            return CreatedAtRoute("GetMessage", new { id = message.Id }, message);
        }

    }
}