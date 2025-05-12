using System.ComponentModel.DataAnnotations;

namespace CaloriesTracker.Models.ViewModels.Account
{
    public class AccountSettingsViewModel
    {
        [Range(500, 10000, ErrorMessage = "Daily calorie goal must be between 500 and 10000.")]
        public decimal DailyCalorieGoal { get; set; }
    }
}