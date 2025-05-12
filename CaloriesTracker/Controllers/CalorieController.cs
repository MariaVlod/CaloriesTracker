using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CaloriesTracker.Models;
using CaloriesTracker.Services;
using System.Security.Claims;


namespace CaloriesTracker.Controllers
{
    [Authorize]
    public class CalorieController : Controller
    {
        private readonly CalorieService _calorieService;
        private readonly ProductService _productService;

        public CalorieController(CalorieService calorieService, ProductService productService)
        {
            _calorieService = calorieService;
            _productService = productService;
        }

        [HttpGet]
        public async Task<IActionResult> Index(DateTime? date)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) return Unauthorized();

            var viewDate = date ?? DateTime.Today;
            var summary = await _calorieService.GetDailySummaryAsync(userId, viewDate);
            var products = await _productService.GetUserProductsAsync(userId);

            ViewBag.Date = viewDate;
            ViewBag.Products = products;

            return View(summary);
        }

        [HttpPost]
        public async Task<IActionResult> AddIntake(int productId, decimal quantity)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) return Unauthorized();

            await _calorieService.AddDailyIntakeAsync(productId, quantity, userId);
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> RemoveIntake(int id)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) return Unauthorized();

            var success = await _calorieService.RemoveDailyIntakeAsync(id, userId);
            if (!success) return NotFound();

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> AddIntakeWithDate(int productId, decimal quantity, DateTime intakeDate)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) return Unauthorized();

            if (intakeDate > DateTime.Today)
            {
                ModelState.AddModelError("intakeDate", "Date cannot be in the future");
                return View("Index", await _calorieService.GetDailySummaryAsync(userId, intakeDate));

            }
            await _calorieService.AddDailyIntakeWithDateAsync(productId, quantity, userId, intakeDate);
            return RedirectToAction("Index", new { date = intakeDate });

        }


        [HttpPost]
        public async Task<IActionResult> EditIntakeQuantity(int id, decimal quantity)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) return Unauthorized();

            var success = await _calorieService.UpdateIntakeQuantityAsync(id, quantity, userId);
            if (!success) return NotFound();

            return RedirectToAction("Index");
        }

    }
}
