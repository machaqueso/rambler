using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Rambler.Services;

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

            if (!accountService.AdminHasPassword().Result)
            {
                return RedirectToAction("Setup");
            }

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

        [AllowAnonymous]
        public async Task<IActionResult> Setup()
        {
            if (await accountService.AdminHasPassword())
            {
                return RedirectToAction("Index", "Home");
            }

            var user = await accountService.FindByUsername("Admin");
            if (user == null)
            {
                throw new InvalidOperationException($"Username 'Admin' not found");
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
    }
}