using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Rambler.Web.Services;

namespace Rambler.Web.Api
{
    [Route("api/[controller]")]
    [ApiController]
    [Area("api")]
    public class UserController : ControllerBase
    {
        private readonly UserService userService;

        public UserController(UserService userService)
        {
            this.userService = userService;
        }

        [Route("")]
        public IActionResult GetUsers()
        {
            var users = userService.GetUsers();
            return Ok(users);
        }

        [Route("token")]
        public IActionResult GetTokens(string apiSource = "")
        {
            var users = userService.GetUsers();
            var tokens = users.SelectMany(x => x.AccessTokens);

            if (!string.IsNullOrEmpty(apiSource))
            {
                tokens = tokens.Where(x => x.ApiSource == apiSource);
            }

            return Ok(tokens.Select(x=>new
            {
                x.Id,
                x.ApiSource,
                x.token_type,
                x.ExpirationDate,
                x.Status,
                x.HasRefreshToken,
                x.User
            }));
        }
    }
}