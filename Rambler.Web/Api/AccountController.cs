using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Rambler.Models;
using Rambler.Models.Exceptions;
using Rambler.Services;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Rambler.Web.Api
{
    [Route("api/[controller]")]
    [ApiController]
    [Area("api")]
    public class AccountController : ControllerBase
    {
        private readonly AccountService accountService;

        public AccountController(AccountService accountService)
        {
            this.accountService = accountService;
        }

        private async Task SignIn(User user)
        {
            var identity = new ClaimsIdentity(CookieAuthenticationDefaults.AuthenticationScheme, ClaimTypes.Name, ClaimTypes.Role);
            identity.AddClaim(new Claim(ClaimTypes.Name, user.UserName));
            identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()));

            if (user.UserName == "Admin")
            {
                identity.AddClaim(new Claim(ClaimTypes.Role, RoleNames.Admin));
            }

            var principal = new ClaimsPrincipal(identity);
            var authProperties = new AuthenticationProperties
            {
                AllowRefresh = true,
                ExpiresUtc = DateTimeOffset.Now.AddDays(1),
                IsPersistent = true,
            };

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(principal), authProperties);
        }

        [Route("login")]
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] User user)
        {
            var foundUser = await accountService.FindByUsername(user.UserName);
            if (foundUser == null)
            {
                return UnprocessableEntity("Invalid username or password");
            }

            if (foundUser.IsLocked)
            {
                return UnprocessableEntity("Account is locked");
            }

            // From this point, password is required
            if (string.IsNullOrEmpty(user.Password))
            {
                return UnprocessableEntity("Password is required");
            }

            if (!accountService.VerifyPassword(foundUser, user.Password))
            {
                return UnprocessableEntity("Invalid username or password");
            }

            await SignIn(foundUser);
            return Ok();
        }

        [AllowAnonymous]
        [Route("{id}/password")]
        [HttpPost]
        public async Task<IActionResult> SetPassword(int id, [FromBody] User user)
        {
            if (await accountService.AdminHasPassword())
            {
                if (User.Identity.Name == null)
                {
                    return Unauthorized();
                }

                var admin = await accountService.FindByUsername(User.Identity.Name);
                if (admin == null)
                {
                    return Unauthorized();
                }

                if (!User.HasClaim(ClaimTypes.Role, RoleNames.Admin))
                {
                    return Forbid();
                }
            }

            var foundUser = await accountService.GetUser(id);
            if (foundUser == null)
            {
                return NotFound();
            }

            foundUser.Password = user.Password;
            foundUser.ConfirmPassword = user.ConfirmPassword;

            try
            {
                await accountService.SetPassword(foundUser);
            }
            catch (UnprocessableEntityException ex)
            {
                return UnprocessableEntity(ex.Message);
            }

            return NoContent();
        }

        [Route("{id}/password")]
        [HttpPut]
        public async Task<IActionResult> ChangePassword(int id, [FromBody] User user)
        {
            var foundUser = await accountService.GetUser(id);
            if (foundUser == null)
            {
                return NotFound();
            }

            foundUser.OldPassword = user.OldPassword;
            foundUser.Password = user.Password;
            foundUser.ConfirmPassword = user.ConfirmPassword;

            try
            {
                await accountService.ChangePassword(foundUser);
            }
            catch (UnprocessableEntityException ex)
            {
                return UnprocessableEntity(ex.Message);
            }

            return NoContent();
        }
    }
}