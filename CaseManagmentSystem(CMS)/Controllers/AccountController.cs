using CaseManagementSystem.ViewModels;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace CaseManagementSystem.Controllers
{
    public class AccountController : Controller
    {
        private readonly SignInManager<Models.ApplicationUser> _signInManager;
        private readonly UserManager<Models.ApplicationUser> _userManager;

        public AccountController(
            SignInManager<Models.ApplicationUser> signInManager,
            UserManager<Models.ApplicationUser> userManager)
        {
            _signInManager = signInManager;
            _userManager = userManager;
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var result = await _signInManager.PasswordSignInAsync(
                model.UserName,
                model.Password,
                model.RememberMe,
                lockoutOnFailure: false);

            if (result.Succeeded)
            {
                var user = await _userManager.FindByNameAsync(model.UserName);

                if (user != null)
                {
                    if (await _userManager.IsInRoleAsync(user, "Supervisor"))
                    {
                        return RedirectToAction("Index", "Dashboard");
                    }

                    if (await _userManager.IsInRoleAsync(user, "Expert"))
                    {
                        return RedirectToAction("MyCases", "Cases");
                    }
                }

                return RedirectToAction("Index", "Home");
            }

            ModelState.AddModelError(
                string.Empty,
                "Invalid username or password.");

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();

            return RedirectToAction(nameof(Login));
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}