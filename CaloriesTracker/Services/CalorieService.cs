using CaloriesTracker.Data;
using CaloriesTracker.Models;
using CaloriesTracker.Models.Stats;
using Microsoft.EntityFrameworkCore;

namespace CaloriesTracker.Services
{
        // Інтерфейс для отримання поточного часу (для кращої тестованості)
        public interface IDateTimeProvider
        {
            DateTime Now { get; }
        }

        // Реалізація інтерфейсу за замовчуванням — повертає системний час
        public class SystemDateTimeProvider : IDateTimeProvider
        {
            public DateTime Now => DateTime.Now;
        }

        public class CalorieService
        {
            private readonly AppDbContext _context;
            private readonly IDateTimeProvider _dateTimeProvider;

            public CalorieService(AppDbContext context, IDateTimeProvider dateTimeProvider)
            {
                _context = context;
                _dateTimeProvider = dateTimeProvider;
            }

            // Приватні методи для обчислення нутрієнтів
            private static decimal CalculateCalories(Product product, decimal quantity)
                => product.CaloriesPer100g * quantity / 100;

            private static decimal CalculateProtein(Product product, decimal quantity)
                => product.ProteinPer100g * quantity / 100;

            private static decimal CalculateFat(Product product, decimal quantity)
                => product.FatPer100g * quantity / 100;

            private static decimal CalculateCarbs(Product product, decimal quantity)
                => product.CarbsPer100g * quantity / 100;

            public async Task<DailyIntake> AddDailyIntakeAsync(int productId, decimal quantity, string userId)
            {
                var intake = new DailyIntake
                {
                    ProductId = productId,
                    Quantity = quantity,
                    UserId = userId,
                    Date = _dateTimeProvider.Now
                };

                _context.DailyIntakes.Add(intake);
                await _context.SaveChangesAsync();
                return intake;
            }

            public async Task<bool> RemoveDailyIntakeAsync(int intakeId, string userId)
            {
                var intake = await _context.DailyIntakes
                    .FirstOrDefaultAsync(d => d.Id == intakeId && d.UserId == userId);

                if (intake == null) return false;

                _context.DailyIntakes.Remove(intake);
                await _context.SaveChangesAsync();
                return true;
            }

            public async Task<DailySummary> GetDailySummaryAsync(string userId, DateTime date)
            {
                var intakes = await _context.DailyIntakes
                    .Include(d => d.Product)
                    .Where(d => d.UserId == userId && d.Date.Date == date.Date)
                    .ToListAsync();

                var user = await _context.Users.FindAsync(userId);

                var totalCalories = intakes.Sum(i => CalculateCalories(i.Product, i.Quantity));
                var totalProtein = intakes.Sum(i => CalculateProtein(i.Product, i.Quantity));
                var totalFat = intakes.Sum(i => CalculateFat(i.Product, i.Quantity));
                var totalCarbs = intakes.Sum(i => CalculateCarbs(i.Product, i.Quantity));

                return new DailySummary
                {
                    Date = date,
                    TotalCalories = totalCalories,
                    TotalProtein = totalProtein,
                    TotalFat = totalFat,
                    TotalCarbs = totalCarbs,
                    DailyGoal = user?.DailyCalorieGoal ?? 2000,
                    Intakes = intakes
                };
            }

            public async Task<UserStats> GetUserStatsAsync(string userId, DateTime startDate, DateTime endDate)
            {
                var intakes = await _context.DailyIntakes
                    .Include(d => d.Product)
                    .Where(d => d.UserId == userId && d.Date >= startDate && d.Date <= endDate)
                    .OrderBy(d => d.Date)
                    .ToListAsync();

                var stats = new UserStats
                {
                    TotalCalories = intakes.Sum(i => CalculateCalories(i.Product, i.Quantity)),
                    TotalProtein = intakes.Sum(i => CalculateProtein(i.Product, i.Quantity)),
                    TotalFat = intakes.Sum(i => CalculateFat(i.Product, i.Quantity)),
                    TotalCarbs = intakes.Sum(i => CalculateCarbs(i.Product, i.Quantity)),
                    DaysTracked = intakes.Select(i => i.Date.Date).Distinct().Count(),
                    PeriodName = $"{startDate:dd.MM.yyyy} - {endDate:dd.MM.yyyy}"
                };

                stats.AverageDailyCalories = intakes.Any()
                    ? intakes.GroupBy(i => i.Date.Date)
                             .Average(g => g.Sum(i => CalculateCalories(i.Product, i.Quantity)))
                    : 0;

                if (intakes.Any())
                {
                    stats.CalorieTrendData = intakes
                        .GroupBy(i => i.Date.Date)
                        .OrderBy(g => g.Key)
                        .Select(g => new CalorieTrendPoint
                        {
                            Date = g.Key,
                            Calories = g.Sum(i => CalculateCalories(i.Product, i.Quantity))
                        })
                        .ToList();

                    stats.MacroNutrientsData = new MacroNutrients
                    {
                        Protein = stats.TotalProtein,
                        Fat = stats.TotalFat,
                        Carbs = stats.TotalCarbs
                    };
                }

                return stats;
            }

            public async Task<DailyIntake> AddDailyIntakeWithDateAsync(int productId, decimal quantity, string userId, DateTime date)
            {
                var intake = new DailyIntake
                {
                    ProductId = productId,
                    Quantity = quantity,
                    UserId = userId,
                    Date = date
                };

                _context.DailyIntakes.Add(intake);
                await _context.SaveChangesAsync();
                return intake;
            }

            public async Task<bool> UpdateIntakeQuantityAsync(int intakeId, decimal newQuantity, string userId)
            {
                var intake = await _context.DailyIntakes
                    .FirstOrDefaultAsync(i => i.Id == intakeId && i.UserId == userId);

                if (intake == null) return false;

                intake.Quantity = newQuantity;
                intake.Date = _dateTimeProvider.Now;
                await _context.SaveChangesAsync();
                return true;
            }
        }
    }