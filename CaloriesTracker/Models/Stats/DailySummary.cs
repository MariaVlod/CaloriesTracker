namespace CaloriesTracker.Models.Stats
{
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
