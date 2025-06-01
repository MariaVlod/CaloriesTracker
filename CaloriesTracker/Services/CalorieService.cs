using CaloriesTracker.Data;
using CaloriesTracker.Models;
using CaloriesTracker.Models.Stats;
using CaloriesTracker.Services.Interfaces;
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
        private readonly ApplicationDbContext _context;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly IDailySummaryService _summaryService;
        private readonly IUserStatsService _userStatsService;

        public CalorieService(
            ApplicationDbContext context,
            IDateTimeProvider dateTimeProvider,
            IDailySummaryService summaryService,
            IUserStatsService userStatsService)
        {
            _context = context;
            _dateTimeProvider = dateTimeProvider;
            _summaryService = summaryService;
            _userStatsService = userStatsService;
        }

        public async Task<List<DailyIntake>> GetDailyIntakeAsync(string userId, DateTime date)
        {
            return await _context.DailyIntakes
                .Include(i => i.Product)
                .Where(i => i.UserId == userId && i.Date.Date == date.Date)
                .ToListAsync();
        }

        public async Task AddIntakeAsync(string userId, int productId, decimal quantity)
        {
            var product = await _context.Products.FindAsync(productId);
            if (product == null) throw new Exception("Product not found.");

            var intake = new DailyIntake
            {
                UserId = userId,
                ProductId = productId,
                Quantity = quantity,
                Date = _dateTimeProvider.Now
            };

            _context.DailyIntakes.Add(intake);
            await _context.SaveChangesAsync();
        }

        public async Task RemoveIntakeAsync(int intakeId)
        {
            var intake = await _context.DailyIntakes.FindAsync(intakeId);
            if (intake != null)
            {
                _context.DailyIntakes.Remove(intake);
                await _context.SaveChangesAsync();
            }
        }

        public async Task UpdateIntakeQuantityAsync(int intakeId, decimal newQuantity)
        {
            var intake = await _context.DailyIntakes.FindAsync(intakeId);
            if (intake != null)
            {
                intake.Quantity = newQuantity;
                intake.Date = _dateTimeProvider.Now;
                await _context.SaveChangesAsync();
            }
        }

        public async Task<DailySummary> GetDailySummaryAsync(string userId, DateTime date, int dailyGoal)
        {
            var intakes = await GetDailyIntakeAsync(userId, date);
            return _summaryService.Generate(intakes, date, dailyGoal);
        }

        public async Task<UserStats> GetUserStatsAsync(string userId, DateTime startDate, DateTime endDate)
        {
            var intakes = await _context.DailyIntakes
                .Include(i => i.Product)
                .Where(i => i.UserId == userId && i.Date.Date >= startDate.Date && i.Date.Date <= endDate.Date)
                .ToListAsync();

            return _userStatsService.Generate(intakes, startDate, endDate);
        }
    }

}