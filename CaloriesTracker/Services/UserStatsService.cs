using CaloriesTracker.Models.Stats;
using CaloriesTracker.Models;
using CaloriesTracker.Services.Interfaces;

namespace CaloriesTracker.Services
{
    public class UserStatsService : IUserStatsService
    {
        private readonly IProductNutritionCalculator _calculator;

        public UserStatsService(IProductNutritionCalculator calculator)
        {
            _calculator = calculator;
        }

        public UserStats Generate(List<DailyIntake> intakes, DateTime start, DateTime end)
        {
            return new UserStats
            {
                StartDate = start,
                EndDate = end,
                TotalCalories = intakes.Sum(i => _calculator.CalculateCalories(i.Product, i.Quantity)),
                TotalProtein = intakes.Sum(i => _calculator.CalculateProtein(i.Product, i.Quantity)),
                TotalFat = intakes.Sum(i => _calculator.CalculateFat(i.Product, i.Quantity)),
                TotalCarbs = intakes.Sum(i => _calculator.CalculateCarbs(i.Product, i.Quantity))
            };
        }
    }

}
