using CaloriesTracker.Models;
using CaloriesTracker.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;


[Authorize]
public class HomeController : Controller
{
    private readonly CalorieService _calorieService;

    public HomeController(CalorieService calorieService)
    {
        _calorieService = calorieService;
    }

    public async Task<IActionResult> Index()
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (userId == null) return Challenge(); // Redirect to login if not authenticated

        var model = await _calorieService.GetDailySummaryAsync(userId, DateTime.Today);
        return View(model);
    }
}