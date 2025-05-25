namespace CaloriesTracker.Models.ViewModels
{
    public class ConsumedProductViewModel
    {
        public string ProductName { get; set; } = "";
        public double Quantity { get; set; }
        public double Calories { get; set; }
        public double Protein { get; set; }
        public double Fat { get; set; }
        public double Carbs { get; set; }
    }
}
