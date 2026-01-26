using AspTelegramBot.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AspTelegramBot.Infrastructure.Database;

/// <summary>
/// Контекст базы данных для работы с сущностями приложения.
/// </summary>
public class AppDbContext : DbContext
{
	public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
	{
	}

	public DbSet<User> Users => Set<User>();

	public DbSet<Role?> Roles { get; set; } = null!;

	public DbSet<UserLog> UserLogs { get; set; }
	public DbSet<RefreshToken> RefreshTokens { get; set; } = null!;

	public DbSet<BotPhrase> BotPhrases { get; set; }

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		base.OnModelCreating(modelBuilder);

		// Many-to-Many User ↔ Role
		modelBuilder.Entity<User>()
		            .HasMany(u => u.Roles)
		            .WithMany(r => r.Users)
		            .UsingEntity(j => j.ToTable("UserRoles")); // название таблицы связей

		modelBuilder.Entity<User>().Property(u => u.RowVersion).IsRowVersion().HasDefaultValue(new byte[8]);
	}
}