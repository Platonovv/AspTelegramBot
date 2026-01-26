using AspTelegramBot.Domain.Entities;
using AspTelegramBot.Domain.Interfaces;
using AspTelegramBot.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace AspTelegramBot.Infrastructure.Repositories;

/// <summary>
/// Репозиторий для управления сущностями типа Role в базе данных.
/// </summary>
public class EfRoleRepository : IRoleRepository
{
	private readonly AppDbContext _dbContext;

	public EfRoleRepository(AppDbContext dbContext)
	{
		_dbContext = dbContext;
	}

	public async Task<IEnumerable<Role?>> GetAllAsync() => await _dbContext.Roles.ToListAsync();

	public async Task<Role?> GetByIdAsync(Guid id) => await _dbContext.Roles.FindAsync(id);

	public async Task<Role?> GetByNameAsync(string roleName)
	{
		if (string.IsNullOrWhiteSpace(roleName))
			throw new ArgumentException("Role name cannot be empty.", nameof(roleName));

		return await _dbContext.Roles
		                       .AsNoTracking() // если только для чтения
		                       .FirstOrDefaultAsync(r => r != null && r.Name == roleName);
	}

	public async Task AddAsync(Role? role)
	{
		await _dbContext.Roles.AddAsync(role);
		await _dbContext.SaveChangesAsync();
	}

	public async Task UpdateAsync(Role? role)
	{
		_dbContext.Roles.Update(role);
		await _dbContext.SaveChangesAsync();
	}

	public async Task DeleteAsync(Role? role)
	{
		_dbContext.Roles.Remove(role);
		await _dbContext.SaveChangesAsync();
	}
}