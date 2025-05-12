using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CaloriesTracker.Models;
using CaloriesTracker.Services;

namespace CaloriesTracker.Controllers
{
    [Authorize]
    public class ReportController : Controller
    {
        private readonly ReportService _reportService;

        public ReportController(ReportService reportService)
        {
            _reportService = reportService;
        }

        [HttpGet]
        public async Task<IActionResult> Daily(DateTime? date)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) return Unauthorized();

            var viewDate = date ?? DateTime.Today;
            var html = await _reportService.GenerateDailyReportHtmlAsync(userId, viewDate);

            return Content(html, "text/html");
        }
    }
}
