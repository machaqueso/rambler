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
        public IActionResult Login()
        {
            return View();
        }

        [AllowAnonymous]
        public async Task< IActionResult >Logout()
        {
            await HttpContext.SignOutAsync();
            return View();
        }
    }
}