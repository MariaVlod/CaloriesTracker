using CaloriesTracker.Models;
using CaloriesTracker.Services.Interfaces;

namespace CaloriesTracker.Services
{
    public class ProductNutritionCalculator : IProductNutritionCalculator
    {
        public decimal CalculateCalories(Product product, decimal quantity) =>
            product.CaloriesPer100g * quantity / 100;

        public decimal CalculateProtein(Product product, decimal quantity) =>
            product.ProteinPer100g * quantity / 100;

        public decimal CalculateFat(Product product, decimal quantity) =>
            product.FatPer100g * quantity / 100;

        public decimal CalculateCarbs(Product product, decimal quantity) =>
            product.CarbsPer100g * quantity / 100;
    }
}
