using Microsoft.AspNetCore.Identity;
namespace CaloriesTracker.Models
{
    public class User : IdentityUser
    {
        public decimal DailyCalorieGoal { get; set; } = 2000;
        public ICollection<DailyIntake> DailyIntakes { get; set; } = new List<DailyIntake>();
    }
}
