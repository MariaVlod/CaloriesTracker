using CaloriesTracker.Models;

namespace CaloriesTracker.Services.Interfaces
{
    public interface ICalorieCalculatorService
    {
        double CalculateCalories(CalorieCalculatorModel model);
    }
}
