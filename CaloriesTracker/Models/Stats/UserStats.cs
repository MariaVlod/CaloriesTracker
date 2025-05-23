namespace CaloriesTracker.Models.Stats
{
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
}
