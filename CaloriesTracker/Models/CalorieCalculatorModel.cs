namespace CaloriesTracker.Models
{
    public class CalorieCalculatorModel
    {
        public string Gender { get; set; } = "male"; // male/female
        public string Goal { get; set; } = "maintain"; // lose/maintain/gain
        public int Age { get; set; } = 25;
        public int Height { get; set; } = 170; // cm
        public double Weight { get; set; } = 70; // kg
        public string ActivityLevel { get; set; } = "moderate"; // sedentary/light/moderate/active/very_active
        public double? CalculatedCalories { get; set; }
    }
}
