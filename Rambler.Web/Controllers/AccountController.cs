using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Rambler.Models;
using Rambler.Services;
using Rambler.Web.Services;

namespace Rambler.Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly AccountService accountService;

        public AccountController(AccountService accountService)
        {
            this.accountService = accountService;
        }

        public IActionResult Index()
        {
            var accounts = accountService.GetUsers();

            return View(accounts);
        }

        [AllowAnonymous]
        public IActionResult Login(string returnUrl)
        {
            ViewData["returnUrl"] = returnUrl;
            return View();
        }

        [AllowAnonymous]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync();
            return View();
        }

        public async Task<IActionResult> LoggedIn(string returnUrl)
        {
            var user = await accountService.FindByUsername(User.Identity.Name);
            if (user == null)
            {
                throw new UnauthorizedAccessException($"Username '{User.Identity.Name}' not found");
            }

            if (user.UserName == "Admin" && !user.LastLoginDate.HasValue && user.MustChangePassword)
            {
                return RedirectToAction("SetPassword", "Account", new { id = user.Id });
            }

            if (user.MustChangePassword)
            {
                return RedirectToAction("ChangePassword", "Account", new { id = user.Id, returnUrl });
            }

            user.LastLoginDate = DateTime.UtcNow;
            await accountService.UpdateUser(user.Id, user);

            if (string.IsNullOrEmpty(returnUrl))
            {
                return RedirectToAction("Index", "Home");
            }

            return LocalRedirect(returnUrl);
        }

        public async Task<IActionResult> SetPassword(int id)
        {
            var user = await accountService.GetUser(id);
            if (user == null)
            {
                throw new UnauthorizedAccessException($"Username '{User.Identity.Name}' not found");
            }

            return View(user);
        }

        public async Task<IActionResult> ChangePassword(int id, string returnUrl)
        {
            var user = await accountService.GetUser(id);
            if (user == null)
            {
                throw new UnauthorizedAccessException($"Username '{User.Identity.Name}' not found");
            }

            return View(user);
        }

        private async Task SignIn(User user)
        {
            var identity = new ClaimsIdentity(CookieAuthenticationDefaults.AuthenticationScheme, ClaimTypes.Name, ClaimTypes.Role);
            identity.AddClaim(new Claim(ClaimTypes.Name, user.UserName));
            identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()));

            //identity.AddClaim(new Claim(ClaimTypes.Role, RoleNames.Admin));

            var principal = new ClaimsPrincipal(identity);
            var authProperties = new AuthenticationProperties
            {
                AllowRefresh = true,
                ExpiresUtc = DateTimeOffset.Now.AddDays(1),
                IsPersistent = true,
            };

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(principal), authProperties);
        }

        [AllowAnonymous]
        public async Task<IActionResult> TokenLogin(string accessToken)
        {
            var user = await accountService.GetUsers()
                .FirstOrDefaultAsync(x => x.AccessTokens.Any(y => y.access_token == accessToken));

            await SignIn(user);
            return RedirectToAction("Index", "Home");
        }

    }
}