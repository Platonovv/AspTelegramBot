using AspTelegramBot.Domain.Entities;
using AspTelegramBot.Domain.Interfaces;
using AspTelegramBot.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace AspTelegramBot.Infrastructure.Repositories;

/// <summary>
/// Репозиторий для работы с пользователями и токенами обновления.
/// </summary>
public class EfUserRepository : IUserRepository
{
	private readonly AppDbContext _dbContext;

	public EfUserRepository(AppDbContext dbContext)
	{
		_dbContext = dbContext;
	}

#region Tokens

	public async Task AddRefreshTokensAsync(RefreshToken refreshToken)
	{
		_dbContext.RefreshTokens.Add(refreshToken);
		await SaveChangesAsync();
	}

#endregion

	public async Task<IEnumerable<User>> GetAllAsync(CancellationToken cancellationToken = default)
	{
		return await _dbContext.Users
		                       .Include(u => u.Roles) // <-- это обязательно
		                       .ToListAsync(cancellationToken);
	}

	public async Task DeleteAllUsersAsync()
	{
		await using var transaction = await BeginTransactionAsync();

		try
		{
			var users = await _dbContext.Users.ToListAsync();
			_dbContext.Users.RemoveRange(users);
			await SaveChangesAsync();
			await transaction.CommitAsync();
		}
		catch
		{
			await transaction.RollbackAsync();
			throw;
		}
	}

	public async Task AddAsync(User user)
	{
		await _dbContext.Users.AddAsync(user);
		await SaveChangesAsync();
	}

	public async Task<User?> GetByIdAsync(Guid id)
		=> await _dbContext.Users
		                   .Include(u => u.Roles) // подтягиваем роли
		                   .FirstOrDefaultAsync(u => u.Id == id);

	public async Task<User?> GetByIdAsync(long id)
		=> await _dbContext.Users
		                   .Include(u => u.Roles) // подтягиваем роли
		                   .FirstOrDefaultAsync(u => u.TelegramID == id);

	public async Task UpdateAsync(User user)
	{
		_dbContext.Users.Update(user);
		await SaveChangesAsync();
	}

	public async Task DeleteAsync(User user)
	{
		_dbContext.Users.Remove(user);
		await SaveChangesAsync();
	}

	public IQueryable<User> Query() => _dbContext.Users.AsQueryable();
	public IQueryable<RefreshToken> QueryTokens() => _dbContext.RefreshTokens.AsQueryable();

	public async Task UpdateToken(RefreshToken refreshToken)
	{
		_dbContext.RefreshTokens.Update(refreshToken);
		await SaveChangesAsync();
	}

	public Task<IDbContextTransaction> BeginTransactionAsync() => _dbContext.Database.BeginTransactionAsync();

	public async Task SaveChangesAsync()
	{
		try
		{
			await _dbContext.SaveChangesAsync();
		}
		catch (DbUpdateConcurrencyException)
		{
			throw;
		}
	}
}