using CaloriesTracker.Models.ViewModels.Account;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using CaloriesTracker.Models;
using CaloriesTracker.Models.Stats;

namespace CaloriesTracker.Services.Interfaces
{
    public interface IUserService
    {
        Task<IdentityResult> RegisterAsync(RegisterViewModel model);
        Task<SignInResult> LoginAsync(string email, string password, bool rememberMe);
        Task LogoutAsync();
        Task<User?> GetUserAsync(string userId);
        Task<decimal> GetCurrentDailyGoalAsync(string userId);
        Task<IdentityResult> UpdateDailyGoalAsync(string userId, decimal newGoal);
        Task<IdentityResult> DeleteUserAsync(string userId);
        UserStats Generate(List<DailyIntake> intakes, DateTime start, DateTime end);

    }
}