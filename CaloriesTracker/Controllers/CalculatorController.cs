using CaloriesTracker.Models;
using Microsoft.AspNetCore.Mvc;

namespace CaloriesTracker.Controllers
{
    public class CalculatorController : Controller
    {
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
                model.CalculatedCalories = CalculateCalories(model);
            }
            return View("Index", model);
        }

        private double CalculateCalories(CalorieCalculatorModel model)
        {
            // Формула Harris-Benedict
            double bmr = model.Gender == "male"
                ? 88.362 + (13.397 * model.Weight) + (4.799 * model.Height) - (5.677 * model.Age)
                : 447.593 + (9.247 * model.Weight) + (3.098 * model.Height) - (4.330 * model.Age);

            double activityMultiplier = model.ActivityLevel switch
            {
                "sedentary" => 1.2,
                "light" => 1.375,
                "moderate" => 1.55,
                "active" => 1.725,
                "very_active" => 1.9,
                _ => 1.375
            };

            double calories = bmr * activityMultiplier;

            // Корекція за ціллю
            calories = model.Goal switch
            {
                "lose" => calories * 0.85,  // Дефіцит 15%
                "gain" => calories * 1.15,  // Профіцит 15%
                _ => calories               // Підтримка
            };

            return Math.Round(calories);
        }
    }
}
