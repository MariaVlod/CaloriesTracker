using CaloriesTracker.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

namespace CaloriesTracker.Data
{
    public class AppDbContext : IdentityDbContext<User>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Product> Products { get; set; }
        public DbSet<DailyIntake> DailyIntakes { get; set; }
        public DbSet<FileRecord> FileRecords { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            // Явно устанавливаем кодировку utf8
            builder.HasCharSet("utf8");

            base.OnModelCreating(builder);

            // Жесткие ограничения для всех Identity таблиц
            const int keyLength = 64; // 64 символа * 3 байта = 192 байта (< 1000)

            foreach (var entity in builder.Model.GetEntityTypes())
            {
                foreach (var property in entity.GetProperties()
                    .Where(p => p.ClrType == typeof(string)))
                {
                    if (property.GetMaxLength() == null)
                        property.SetMaxLength(keyLength);
                }
            }

            // Ваши кастомные конфигурации
            builder.Entity<Product>()
                .HasOne(p => p.User)
                .WithMany()
                .HasForeignKey(p => p.UserId);

            builder.Entity<DailyIntake>()
                .HasOne(d => d.User)
                .WithMany(u => u.DailyIntakes)
                .HasForeignKey(d => d.UserId);

            builder.Entity<FileRecord>()
                .HasOne(f => f.User)
                .WithMany()
                .HasForeignKey(f => f.UserId);
        }
    }
}
