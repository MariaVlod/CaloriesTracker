using CaloriesTracker.Models.Stats;
using CaloriesTracker.Models;

namespace CaloriesTracker.Services.Interfaces
{
    public interface IUserStatsService
    {
        UserStats Generate(List<DailyIntake> intakes, DateTime start, DateTime end);
    }
}
