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

        public AccountController(IUserService userService, ILogger<AccountController> logger)
        {
            _userService = userService;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Register(decimal? calories = null)
        {
            var model = new RegisterViewModel
            {
                DailyCalorieGoal = calories ?? 2000m
            };
            return View(model);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var result = await _userService.RegisterAsync(model);
            if (result.Succeeded)
            {
                _logger.LogInformation("User {Email} registered.", model.Email);
                await _userService.LoginAsync(model.Email, model.Password, rememberMe: false);
                return RedirectToHome();
            }

            AddErrors(result.Errors, model.Email);
            return View(model);
        }

        [HttpGet]
        public IActionResult Login(string? returnUrl)
        {
            TempData["ReturnUrl"] = returnUrl;
            return View(new LoginViewModel());
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            string? returnUrl = TempData["ReturnUrl"] as string;
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
                    "Account is locked due to multiple failed login attempts.");
                _logger.LogWarning("Lockout for {Email}.", model.Email);
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                _logger.LogWarning("Invalid login attempt for {Email}.", model.Email);
            }

            return View(model);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _userService.LogoutAsync();
            _logger.LogInformation("User logged out.");
            return RedirectToHome();
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            string? userId = GetUserId();
            if (userId == null) return Unauthorized();

            var goal = await _userService.GetCurrentDailyGoalAsync(userId);
            var vm = new AccountSettingsViewModel { DailyCalorieGoal = goal };
            return View(vm);
        }

        [Authorize]
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateDailyGoal(AccountSettingsViewModel model)
        {
            if (!ModelState.IsValid) return View("Index", model);

            string? userId = GetUserId();
            if (userId == null) return Unauthorized();

            var result = await _userService.UpdateDailyGoalAsync(userId, model.DailyCalorieGoal);
            if (result.Succeeded)
            {
                _logger.LogInformation("User {UserId} updated daily goal to {Goal}.",
                    userId, model.DailyCalorieGoal);
                return RedirectToHome();
            }

            AddErrors(result.Errors, userId);
            return View("Index", model);
        }

        [Authorize]
        [HttpGet]
        public IActionResult DeleteAccount() => View();

        [Authorize]
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteAccountConfirmed()
        {
            string? userId = GetUserId();
            if (userId == null) return Unauthorized();

            var result = await _userService.DeleteUserAsync(userId);
            if (result.Succeeded)
            {
                await _userService.LogoutAsync();
                _logger.LogInformation("User {UserId} deleted their account.", userId);
                return RedirectToHome();
            }

            AddErrors(result.Errors, userId);
            return View("DeleteAccount");
        }
        private string? GetUserId() =>
            User.FindFirstValue(ClaimTypes.NameIdentifier);

        private IActionResult RedirectToHome() =>
            RedirectToAction(nameof(HomeController.Index), "Home");

        private IActionResult RedirectToLocal(string? returnUrl) =>
            Url.IsLocalUrl(returnUrl) ? Redirect(returnUrl) : RedirectToHome();

        private void AddErrors(IEnumerable<IdentityError> errors, string keyContext)
        {
            foreach (var err in errors)
            {
                ModelState.AddModelError(string.Empty, err.Description);
                _logger.LogWarning("Error ({Context}): {Description}", keyContext, err.Description);
            }
        }
    }
}
