using CaloriesTracker.Data;
using CaloriesTracker.Models;
using CaloriesTracker.Models.ViewModels.Account;
using CaloriesTracker.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace CaloriesTracker.Services
{
    public class UserService : IUserService
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly AppDbContext _context;
        private readonly ILogger<UserService> _logger;

        public UserService(
            UserManager<User> userManager,
            SignInManager<User> signInManager,
            AppDbContext context,
            ILogger<UserService> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
            _logger = logger;
        }

        public async Task<IdentityResult> RegisterAsync(RegisterViewModel model)
        {
            var user = new User
            {
                UserName = model.Email,
                Email = model.Email,
                DailyCalorieGoal = model.DailyCalorieGoal
            };
            return await _userManager.CreateAsync(user, model.Password);
        }

        public async Task<SignInResult> LoginAsync(string email, string password, bool rememberMe)
        {
            return await _signInManager.PasswordSignInAsync(
                email, password, rememberMe, lockoutOnFailure: true);
        }

        public async Task LogoutAsync()
        {
            await _signInManager.SignOutAsync();
        }

        public async Task<User?> GetUserAsync(string userId)
        {
            return await _userManager.FindByIdAsync(userId);
        }

        public async Task<decimal> GetCurrentDailyGoalAsync(string userId)
        {
            var user = await GetUserAsync(userId);
            return user?.DailyCalorieGoal ?? 2000;
        }

        public async Task<IdentityResult> UpdateDailyGoalAsync(string userId, decimal newGoal)
        {
            var user = await GetUserAsync(userId);
            if (user == null)
            {
                return IdentityResult.Failed(new IdentityError { Description = "User not found" });
            }

            user.DailyCalorieGoal = newGoal;
            return await _userManager.UpdateAsync(user);
        }

        public async Task<IdentityResult> DeleteUserAsync(string userId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var user = await GetUserAsync(userId);
                if (user == null)
                {
                    return IdentityResult.Failed(new IdentityError { Description = "User not found" });
                }

                await _context.Products.Where(p => p.UserId == userId).ExecuteDeleteAsync();
                await _context.DailyIntakes.Where(d => d.UserId == userId).ExecuteDeleteAsync();
                await _context.FileRecords.Where(f => f.UserId == userId).ExecuteDeleteAsync();

                var result = await _userManager.DeleteAsync(user);
                if (result.Succeeded)
                {
                    await transaction.CommitAsync();
                }
                return result;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }
}