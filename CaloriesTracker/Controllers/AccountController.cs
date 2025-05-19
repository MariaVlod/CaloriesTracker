using CaloriesTracker.Models;
using CaloriesTracker.Models.ViewModels.Account;
using CaloriesTracker.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CaloriesTracker.Controllers
{
    public class AccountController : Controller
    {
        private readonly IUserService _userService;
        private readonly ILogger<AccountController> _logger;

        public AccountController(
            IUserService userService,
            ILogger<AccountController> logger)
        {
            _userService = userService;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Register(double? calories = null)
        {
            var model = new RegisterViewModel 
            {
                DailyCalorieGoal = calories.HasValue ? (decimal)calories.Value : 2000
            };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var result = await _userService.RegisterAsync(model);
            
            if (result.Succeeded)
            {
                _logger.LogInformation("User {Email} registered successfully.", model.Email);
                await _userService.LoginAsync(model.Email, model.Password, false);
                return RedirectToHome();
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
                _logger.LogWarning("Registration error for {Email}: {Error}", 
                    model.Email, error.Description);
            }
            return View(model);
        }

        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View(new LoginViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            if (!ModelState.IsValid) return View(model);

            var result = await _userService.LoginAsync(model.Email, model.Password, model.RememberMe);

            if (result.Succeeded)
            {
                _logger.LogInformation("User {Email} logged in.", model.Email);
                return RedirectToLocal(returnUrl);
            }

            if (result.IsLockedOut)
            {
                ModelState.AddModelError(string.Empty, 
                    "Account locked out due to multiple failed attempts. Try again later.");
                _logger.LogWarning("Account lockout for {Email}", model.Email);
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                _logger.LogWarning("Invalid login attempt for {Email}", model.Email);
            }
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _userService.LogoutAsync();
            _logger.LogInformation("User logged out.");
            return RedirectToHome();
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateDailyGoal(decimal newGoal)
        {
            var userId = GetCurrentUserId();
            if (userId == null) return Unauthorized();

            var result = await _userService.UpdateDailyGoalAsync(userId, newGoal);
            
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return View("Index");
            }
            return RedirectToHome();
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var userId = GetCurrentUserId();
            if (userId == null) return Unauthorized();

            var currentGoal = await _userService.GetCurrentDailyGoalAsync(userId);
            return View(new AccountSettingsViewModel { DailyCalorieGoal = currentGoal });
        }

        [Authorize]
        [HttpGet]
        public IActionResult DeleteAccount()
        {
            return View();
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteAccountConfirmed()
        {
            var userId = GetCurrentUserId();
            if (userId == null) return Unauthorized();

            var result = await _userService.DeleteUserAsync(userId);
            if (result.Succeeded)
            {
                await _userService.LogoutAsync();
                return RedirectToHome();
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            return View("DeleteAccount");
        }

        private string? GetCurrentUserId() => 
            User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        private IActionResult RedirectToHome() => 
            RedirectToAction("Index", "Home");

        private IActionResult RedirectToLocal(string? returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToHome();
        }
    }
}