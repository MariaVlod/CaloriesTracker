using CaloriesTracker.Models;
using Microsoft.AspNetCore.Mvc;
using CaloriesTracker.Services.Interfaces;

namespace CaloriesTracker.Controllers
{
    public class CalculatorController : Controller
    {
        private readonly ICalorieCalculatorService _calculatorService;

        public CalculatorController(ICalorieCalculatorService calculatorService)
        {
            _calculatorService = calculatorService;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View(new CalorieCalculatorModel());
        }

        [HttpPost]
        public IActionResult Calculate(CalorieCalculatorModel model)
        {
            if (ModelState.IsValid)
            {
                model.CalculatedCalories = _calculatorService.CalculateCalories(model);
            }
            return View("Index", model);
        }
    }
}
