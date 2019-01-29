using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Rambler.Models;
using Rambler.Services;

namespace Rambler.Web.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly AccountService accountService;

        public AccountController(AccountService accountService)
        {
            this.accountService = accountService;
        }

        [Route("login")]
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] User user)
        {
            var foundUser = await accountService.FindByUsername(user.UserName);
            if (foundUser == null)
            {
                return NotFound();
            }

            if (foundUser.IsLocked)
            {
                return Forbid("Account is locked");
            }

            if (foundUser.LastLoginDate.HasValue && !foundUser.MustChangePassword)
            {
                if (!accountService.VerifyPassword(foundUser, user.Password))
                {
                    return Unauthorized();
                }
            }

            var claims = new List<Claim>();
            var userIdentity = new ClaimsIdentity(claims, "login");
            var principal = new ClaimsPrincipal(userIdentity);

            await HttpContext.SignInAsync(principal);

            return Ok();
        }
    }
}