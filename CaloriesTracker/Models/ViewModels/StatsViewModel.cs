using static CaloriesTracker.Services.CalorieService;

namespace CaloriesTracker.Models.ViewModels
{
    public class StatsViewModel
    {
        public UserStats TwoWeeksStats { get; set; }
        public UserStats MonthStats { get; set; }
        public UserStats YearStats { get; set; }
        public string SelectedPeriod { get; set; }
    }
}
