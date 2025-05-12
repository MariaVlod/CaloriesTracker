using CaloriesTracker.Data;
using CaloriesTracker.Models;
using Microsoft.AspNetCore.Identity;
using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;

namespace CaloriesTracker.Services
{
    public class UserService
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

            // Налаштування політик паролів
            _userManager.Options.Password.RequiredLength = 6;
            _userManager.Options.Password.RequireDigit = true;
            _userManager.Options.Password.RequireLowercase = true;
            _userManager.Options.Password.RequireUppercase = false;
            _userManager.Options.Password.RequireNonAlphanumeric = false;

            // Унікальність email
            _userManager.Options.User.RequireUniqueEmail = true;
        }

        public async Task<IdentityResult> RegisterAsync(string email, string password, decimal dailyCalorieGoal)
        {
            try
            {
                var user = new User
                {
                    UserName = email,
                    Email = email,
                    DailyCalorieGoal = dailyCalorieGoal
                };

                var result = await _userManager.CreateAsync(user, password);

                if (result.Succeeded)
                {
                    _logger.LogInformation("User {Email} registered successfully.", email);
                }
                else
                {
                    _logger.LogWarning("Failed to register user {Email}. Errors: {Errors}",
                        email, string.Join(", ", result.Errors.Select(e => e.Description)));
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error registering user {Email}", email);
                throw;
            }
        }

        public async Task<SignInResult> LoginAsync(string email, string password, bool rememberMe)
        {
            try
            {
                var result = await _signInManager.PasswordSignInAsync(
                    email, password, rememberMe, lockoutOnFailure: true);

                if (result.Succeeded)
                {
                    _logger.LogInformation("User {Email} logged in.", email);
                }
                else
                {
                    _logger.LogWarning("Failed login attempt for {Email}. Result: {Result}",
                        email, result.ToString());
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error logging in user {Email}", email);
                throw;
            }
        }

        public async Task LogoutAsync()
        {
            try
            {
                await _signInManager.SignOutAsync();
                _logger.LogInformation("User logged out.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error logging out");
                throw;
            }
        }

        [return: MaybeNull]
        public async Task<User> GetUserAsync(string userId)
        {
            try
            {
                return await _userManager.FindByIdAsync(userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user {UserId}", userId);
                throw;
            }
        }

        public async Task<decimal> GetCurrentDailyGoalAsync(string userId)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                return user?.DailyCalorieGoal ?? 2000;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting daily goal for user {UserId}", userId);
                throw;
            }
        }

        public async Task<IdentityResult> UpdateDailyGoalAsync(string userId, decimal newGoal)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    _logger.LogWarning("User {UserId} not found for daily goal update", userId);
                    return IdentityResult.Failed(new IdentityError
                    {
                        Description = "User not found"
                    });
                }

                user.DailyCalorieGoal = newGoal;
                var result = await _userManager.UpdateAsync(user);

                if (result.Succeeded)
                {
                    _logger.LogInformation("Updated daily goal for user {UserId} to {Goal}",
                        userId, newGoal);
                }
                else
                {
                    _logger.LogWarning("Failed to update daily goal for user {UserId}. Errors: {Errors}",
                        userId, string.Join(", ", result.Errors.Select(e => e.Description)));
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating daily goal for user {UserId}", userId);
                throw;
            }
        }

        public async Task<IdentityResult> DeleteUserAsync(string userId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    _logger.LogWarning("User {UserId} not found for deletion", userId);
                    return IdentityResult.Failed(new IdentityError
                    {
                        Description = "User not found"
                    });
                }

                // Видаляємо всі пов'язані дані в одній транзакції
                await _context.Products.Where(p => p.UserId == userId).ExecuteDeleteAsync();
                await _context.DailyIntakes.Where(d => d.UserId == userId).ExecuteDeleteAsync();
                await _context.FileRecords.Where(f => f.UserId == userId).ExecuteDeleteAsync();

                var result = await _userManager.DeleteAsync(user);

                if (result.Succeeded)
                {
                    await transaction.CommitAsync();
                    _logger.LogInformation("User {UserId} deleted successfully", userId);
                }
                else
                {
                    await transaction.RollbackAsync();
                    _logger.LogWarning("Failed to delete user {UserId}. Errors: {Errors}",
                        userId, string.Join(", ", result.Errors.Select(e => e.Description)));
                }

                return result;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error deleting user {UserId}", userId);
                throw;
            }
        }
    }
}