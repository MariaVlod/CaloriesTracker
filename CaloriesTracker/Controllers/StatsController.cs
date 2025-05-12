using CaloriesTracker.Models;
using CaloriesTracker.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CaloriesTracker.Models.ViewModels;
using System.Security.Claims;
using static CaloriesTracker.Services.CalorieService;

namespace CaloriesTracker.Controllers
{
    [Authorize]
    public class StatsController : Controller
    {
        private readonly CalorieService _calorieService;

        public StatsController(CalorieService calorieService)
        {
            _calorieService = calorieService;
        }

        public async Task<IActionResult> Index(string period = "2weeks")
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) return Unauthorized();

            var now = DateTime.Today;
            var stats = new StatsViewModel();

            // Останні 2 тижні
            stats.TwoWeeksStats = await _calorieService.GetUserStatsAsync(
                userId,
                now.AddDays(-14),
                now);

            // Останній місяць
            stats.MonthStats = await _calorieService.GetUserStatsAsync(
                userId,
                now.AddMonths(-1),
                now);

            // Останній рік
            stats.YearStats = await _calorieService.GetUserStatsAsync(
                userId,
                now.AddYears(-1),
                now);

            stats.SelectedPeriod = period;
            return View(stats);
        }
    }
}
