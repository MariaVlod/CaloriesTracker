namespace CaloriesTracker.Models.ViewModels
{
    public class DailyReportViewModel
    {
        public DateTime Date { get; set; }
        public double TotalCalories { get; set; }
        public double DailyGoal { get; set; }
        public double TotalProtein { get; set; }
        public double TotalFat { get; set; }
        public double TotalCarbs { get; set; }
        public List<ConsumedProductViewModel> ConsumedProducts { get; set; } = new();
    }
}
