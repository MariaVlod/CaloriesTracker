using CaloriesTracker.Models;
using CaloriesTracker.Services.Interfaces;
using System;

namespace CaloriesTracker.Services
{
    public class CalorieCalculatorService : ICalorieCalculatorService
    {
        public double CalculateCalories(CalorieCalculatorModel model)
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
