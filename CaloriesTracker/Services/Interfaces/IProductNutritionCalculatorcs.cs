using CaloriesTracker.Models;

namespace CaloriesTracker.Services.Interfaces
{
    public interface IProductNutritionCalculator
    {
        decimal CalculateCalories(Product product, decimal quantity);
        decimal CalculateProtein(Product product, decimal quantity);
        decimal CalculateFat(Product product, decimal quantity);
        decimal CalculateCarbs(Product product, decimal quantity);
    }
}
