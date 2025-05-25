using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CaloriesTracker.Services;
using CaloriesTracker.Models.ViewModels;

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
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized();

            var viewDate = date ?? DateTime.Today;
            DailyReportViewModel vm = await _reportService.GetDailyReportAsync(userId, viewDate);
            return View(vm);   
        }
    }
}
