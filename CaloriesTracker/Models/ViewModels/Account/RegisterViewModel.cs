using System.ComponentModel.DataAnnotations;

namespace CaloriesTracker.Models.ViewModels.Account
{
    public class RegisterViewModel
    {
        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        [RegularExpression(@"^(?=.*\d).+$", ErrorMessage = "Password must contain at least one number.")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }

        [Required]
        [Range(500, 10000, ErrorMessage = "Daily calorie goal must be between 500 and 10000.")]
        [Display(Name = "Daily Calorie Goal")]
        public decimal DailyCalorieGoal { get; set; } = 2000;


    }
}