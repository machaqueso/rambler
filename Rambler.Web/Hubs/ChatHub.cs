using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Rambler.Models;
using Rambler.Services;
using Rambler.Web.Services;

namespace Rambler.Web.Hubs
{
    public class ChatHub : Hub
    {
        private readonly ChatService chatService;
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly AuthorService authorService;

        public ChatHub(ChatService chatService, IHttpContextAccessor httpContextAccessor, AuthorService authorService)
        {
            this.chatService = chatService;
            this.httpContextAccessor = httpContextAccessor;
            this.authorService = authorService;
        }

        public bool IsValidAuthor(Author author)
        {
            return author != null
                   && !string.IsNullOrWhiteSpace(author.Source)
                   && !string.IsNullOrWhiteSpace(author.SourceAuthorId)
                   && !string.IsNullOrWhiteSpace(author.Name);
        }

        public async Task<Author> GetOrBuildAuthorFromClaims()
        {
            Author author = null;

            // Try get author from NameIdentifier claim
            var nameIdentifierClaim = httpContextAccessor.HttpContext.User.Claims.SingleOrDefault(x => x.Type == ClaimTypes.NameIdentifier);
            if (nameIdentifierClaim != null)
            {
                author = await authorService.GetAuthors()
                    .Where(x => x.Source == ApiSource.Rambler)
                    .FirstOrDefaultAsync(x => x.SourceAuthorId == nameIdentifierClaim.Value);
            }
            if (IsValidAuthor(author))
            {
                return author;
            }

            // Try get author from Name and source claim
            var nameClaim = httpContextAccessor.HttpContext.User.Claims.SingleOrDefault(x => x.Type == ClaimTypes.Name);
            if (nameClaim != null)
            {
                // First try to complete existing record if any
                if (author != null)
                {
                    author.Name = nameClaim.Value;
                    if (IsValidAuthor(author))
                    {
                        return author;
                    }
                }

                author = await authorService.GetAuthors()
                    .Where(x => x.Source == ApiSource.Rambler)
                    .FirstOrDefaultAsync(x => x.Name == nameClaim.Value);
            }

            if (author != null && string.IsNullOrWhiteSpace(author.Source))
            {
                author.Source = ApiSource.Rambler;
            }
            if (IsValidAuthor(author))
            {
                return author;
            }

            // Create new author from claims
            author = new Author
            {
                SourceAuthorId = nameIdentifierClaim.Value,
                Name = nameClaim.Value,
                Source = ApiSource.Rambler
            };
            return author;
        }

        public async Task SendMessage(string author, string message)
        {
            var chatMessage = new ChatMessage
            {
                Date = DateTime.UtcNow,
                Message = message,
                Source = ApiSource.Rambler
            };

            if (!string.IsNullOrWhiteSpace(author))
            {
                chatMessage.Author = await authorService.GetAuthors().SingleOrDefaultAsync(x => x.Name == author);
            }

            if (chatMessage.Author == null)
            {
                chatMessage.Author = await GetOrBuildAuthorFromClaims();
            }

            await chatService.CreateMessage(chatMessage);
        }

        public async Task DirectMessage(string author, string message)
        {
            var chatMessage = new ChatMessage
            {
                Source = ApiSource.Rambler,
                Date = DateTime.UtcNow,
                Message = message
            };

            await Clients.All.SendAsync("ReceiveChannelMessage", new ChannelMessage
            {
                Channel = "All",
                ChatMessage = chatMessage
            });
        }

        public async Task TestMessage()
        {
            await Clients.All.SendAsync("ReceiveTestMessage", new
            {
                Date = DateTime.UtcNow,
                Author = "Test",
                Message = Guid.NewGuid().ToString()
            });
        }
    }
}