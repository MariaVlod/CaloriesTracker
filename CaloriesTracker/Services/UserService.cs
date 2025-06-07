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
            _userManager    = userManager;
            _signInManager  = signInManager;
            _context        = context;
            _logger         = logger;
        }

        public Task<IdentityResult> RegisterAsync(RegisterViewModel model)
        {
            var user = new User
            {
                UserName = model.Email,
                Email    = model.Email,
                DailyCalorieGoal = model.DailyCalorieGoal
            };
            return _userManager.CreateAsync(user, model.Password);
        }

        public Task<SignInResult> LoginAsync(string email, string password, bool rememberMe)
            => _signInManager.PasswordSignInAsync(email, password, rememberMe, lockoutOnFailure: true);

        public Task LogoutAsync()
            => _signInManager.SignOutAsync();

        public Task<User?> GetUserAsync(string userId)
            => _userManager.FindByIdAsync(userId);

        public async Task<decimal> GetCurrentDailyGoalAsync(string userId)
        {
            var user = await GetUserOrThrowAsync(userId);
            return user.DailyCalorieGoal;
        }

        public async Task<IdentityResult> UpdateDailyGoalAsync(string userId, decimal newGoal)
        {
            var user = await GetUserOrThrowAsync(userId);
            user.DailyCalorieGoal = newGoal;
            return await _userManager.UpdateAsync(user);
        }

        public async Task<IdentityResult> DeleteUserAsync(string userId)
        {
            var user = await GetUserOrThrowAsync(userId);

            await using var tx = await _context.Database.BeginTransactionAsync();
            try
            {
                await DeleteUserRelatedDataAsync(userId);

                var result = await _userManager.DeleteAsync(user);
                if (result.Succeeded)
                    await tx.CommitAsync();

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting user {UserId}", userId);
                await tx.RollbackAsync();
                throw;
            }
        }


        private async Task<User> GetUserOrThrowAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                throw new KeyNotFoundException($"User with ID '{userId}' not found.");
            return user;
        }

        private Task DeleteUserRelatedDataAsync(string userId)
        {
            return Task.WhenAll(
                _context.Products.Where(p => p.UserId == userId).ExecuteDeleteAsync(),
                _context.DailyIntakes.Where(d => d.UserId == userId).ExecuteDeleteAsync(),
                _context.FileRecords.Where(f => f.UserId == userId).ExecuteDeleteAsync()
            );
        }
    }
}
