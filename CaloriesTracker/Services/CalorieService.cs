using CaloriesTracker.Data;
using CaloriesTracker.Models;
using Microsoft.EntityFrameworkCore;

namespace CaloriesTracker.Services
{
    public class CalorieService
    {
        private readonly AppDbContext _context;

        public CalorieService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<DailyIntake> AddDailyIntakeAsync(int productId, decimal quantity, string userId)
        {
            var intake = new DailyIntake
            {
                ProductId = productId,
                Quantity = quantity,
                UserId = userId,
                Date = DateTime.Now
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

            var summary = new DailySummary
            {
                Date = date,
                TotalCalories = intakes.Sum(i => i.Product.CaloriesPer100g * i.Quantity / 100),
                TotalProtein = intakes.Sum(i => i.Product.ProteinPer100g * i.Quantity / 100),
                TotalFat = intakes.Sum(i => i.Product.FatPer100g * i.Quantity / 100),
                TotalCarbs = intakes.Sum(i => i.Product.CarbsPer100g * i.Quantity / 100),
                DailyGoal = user?.DailyCalorieGoal ?? 2000,
                Intakes = intakes
            };

            return summary;
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
                TotalCalories = intakes.Sum(i => i.Product.CaloriesPer100g * i.Quantity / 100),
                TotalProtein = intakes.Sum(i => i.Product.ProteinPer100g * i.Quantity / 100),
                TotalFat = intakes.Sum(i => i.Product.FatPer100g * i.Quantity / 100),
                TotalCarbs = intakes.Sum(i => i.Product.CarbsPer100g * i.Quantity / 100),
                DaysTracked = intakes.Select(i => i.Date.Date).Distinct().Count(),
                PeriodName = $"{startDate:dd.MM.yyyy} - {endDate:dd.MM.yyyy}"
            };

            stats.AverageDailyCalories = intakes.Any()
                ? intakes.GroupBy(i => i.Date.Date)
                         .Average(g => g.Sum(i => i.Product.CaloriesPer100g * i.Quantity / 100))
                : 0;

            if (intakes.Any())
            {
                stats.CalorieTrendData = intakes
                    .GroupBy(i => i.Date.Date)
                    .OrderBy(g => g.Key)
                    .Select(g => new CalorieTrendPoint
                    {
                        Date = g.Key,
                        Calories = g.Sum(i => i.Product.CaloriesPer100g * i.Quantity / 100)
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
            intake.Date = DateTime.Now;
            await _context.SaveChangesAsync();
            return true;
        }


        public class UserStats
        {
            public decimal TotalCalories { get; set; }
            public decimal AverageDailyCalories { get; set; }
            public int DaysTracked { get; set; }
            public string PeriodName { get; set; }
            public List<CalorieTrendPoint> CalorieTrendData { get; set; } = new();
            public MacroNutrients MacroNutrientsData { get; set; }
            public decimal TotalProtein { get; set; }
            public decimal TotalFat { get; set; }
            public decimal TotalCarbs { get; set; }
        }

        public class CalorieTrendPoint
        {
            public DateTime Date { get; set; }
            public decimal Calories { get; set; }
        }

        public class MacroNutrients
        {
            public decimal Protein { get; set; }
            public decimal Fat { get; set; }
            public decimal Carbs { get; set; }
        }

        public class DailySummary
        {
            public DateTime Date { get; set; }
            public decimal TotalCalories { get; set; }
            public decimal TotalProtein { get; set; }
            public decimal TotalFat { get; set; }
            public decimal TotalCarbs { get; set; }
            public decimal DailyGoal { get; set; }
            public List<DailyIntake> Intakes { get; set; } = new();
        }
    }
}
