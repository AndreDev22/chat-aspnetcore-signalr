using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ChatApp.Models;
using Microsoft.AspNetCore.Identity;

namespace ChatApp.Controllers
{
    public class AccountController : Controller
    {
        private SignInManager<User> signInManager;
        private UserManager<User> userManager;

        public AccountController(UserManager<User> _userManager, SignInManager<User> _signInManager)
        {
            userManager = _userManager;
            signInManager = _signInManager;
        }

        [HttpGet]
        public IActionResult Login() => View();
        [HttpPost]
        public async Task<IActionResult> Login(string username, string password)
        {
            if (!(string.IsNullOrEmpty(username) && string.IsNullOrEmpty(password)))
            {
                var result = await signInManager.PasswordSignInAsync(username, password, false, false);

                if (result.Succeeded)
                {
                    return RedirectToAction("Index", "Home");
                }
            }
            return RedirectToAction("Index", "Account");
        }


        [HttpGet]
        public IActionResult Register() => View();
        [HttpPost]
        public async Task<IActionResult> Register(string username, string password)
        {
            var user = new User
            {
                UserName = username
            };
            var result = await userManager.CreateAsync(user, password);

            if (result.Succeeded)
            {
                await signInManager.SignInAsync(user, false);
                return RedirectToAction("Index", "Home");
            }

            return RedirectToAction("Register", "Account");
        }

        public async Task<IActionResult> Logout()
        {
            await signInManager.SignOutAsync();
            return RedirectToAction("Login", "Account");
        }
    }
}