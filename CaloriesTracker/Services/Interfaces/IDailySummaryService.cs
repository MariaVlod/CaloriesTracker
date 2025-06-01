using CaloriesTracker.Models.Stats;
using CaloriesTracker.Models;

namespace CaloriesTracker.Services.Interfaces
{
    public interface IDailySummaryService
    {
        DailySummary Generate(List<DailyIntake> intakes, DateTime date, int dailyGoal);
    }
}
