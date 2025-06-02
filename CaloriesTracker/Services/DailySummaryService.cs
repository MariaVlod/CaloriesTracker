using CaloriesTracker.Models.Stats;
using CaloriesTracker.Models;
using CaloriesTracker.Services.Interfaces;

namespace CaloriesTracker.Services
{
    public class DailySummaryService : IDailySummaryService
    {
        private readonly IProductNutritionCalculator _calculator;

        public DailySummaryService(IProductNutritionCalculator calculator)
        {
            _calculator = calculator;
        }

        public DailySummary Generate(List<DailyIntake> intakes, DateTime date, int dailyGoal)
        {
            var summary = new DailySummary
            {
                Date = date,
                Calories = intakes.Sum(i => _calculator.CalculateCalories(i.Product, i.Quantity)),
                Protein = intakes.Sum(i => _calculator.CalculateProtein(i.Product, i.Quantity)),
                Fat = intakes.Sum(i => _calculator.CalculateFat(i.Product, i.Quantity)),
                Carbs = intakes.Sum(i => _calculator.CalculateCarbs(i.Product, i.Quantity)),
                DailyGoal = dailyGoal
            };
            return summary;
        }
    }

}
