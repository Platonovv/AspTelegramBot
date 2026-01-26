using AspTelegramBot.Application.DTOs.Users;
using AspTelegramBot.Application.Interfaces.ForUser;
using AspTelegramBot.Application.Settings;
using AspTelegramBot.Domain.Entities;
using AspTelegramBot.Domain.Interfaces;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace AspTelegramBot.Application.Services.ForUser;

/// <summary>
/// Предоставляет набор методов для управления и взаимодействия с сущностями пользователей.
/// Этот сервис позволяет выполнять такие операции, как извлечение, добавление, обновление, удаление и поиск пользователей.
/// </summary>
public class UserService : IUserService
{
	private readonly IMapper _mapper;
	private readonly IMemoryCache _cache;
	private readonly DatabaseSettings _dbSettings;
	private readonly IUserRepository _userRepository;

	public UserService(IUserRepository userRepository,
	                   IOptions<DatabaseSettings> dbOptions,
	                   IMapper mapper,
	                   IMemoryCache cache)
	{
		_cache = cache;
		_mapper = mapper;
		_dbSettings = dbOptions.Value;
		_userRepository = userRepository;
	}

	public string GetConnectionInfo()
	{
		return _dbSettings.ConnectionString;
	}

#region Database

	public async Task<IEnumerable<UserResponseDto>?> GetAllUsersAsync(CancellationToken cancellationToken = default)
	{
		const string cacheKey = "all_users";

		if (_cache.TryGetValue(cacheKey, out List<UserResponseDto>? cachedUsers))
			return cachedUsers;

		var users = await _userRepository.GetAllAsync(cancellationToken);
		cachedUsers = _mapper.Map<List<UserResponseDto>>(users);

		var cacheOptions = new MemoryCacheEntryOptions
		{
			AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(30), SlidingExpiration = TimeSpan.FromSeconds(10)
		};

		_cache.Set(cacheKey, cachedUsers, cacheOptions);

		return cachedUsers;
	}

	public async Task<User?> GetUserByIdAsync(Guid id) => await _userRepository.GetByIdAsync(id);
	public async Task<User?> GetUserByIdAsync(long id) => await _userRepository.GetByIdAsync(id);

	public async Task<User> UpdateUserWithConcurrencyCheckAsync(User user)
	{
		try
		{
			await _userRepository.UpdateAsync(user);
			return user;
		}
		catch (DbUpdateConcurrencyException ex)
		{
			// Можно логировать и бросить детализированное исключение
			throw new InvalidOperationException(
				"Конфликт при обновлении пользователя. Данные были изменены другим пользователем.",
				ex);
		}
	}

	public async Task<User> UpdateUserAsync(User user)
	{
		await _userRepository.UpdateAsync(user);
		return user;
	}

	public async Task DeleteAllUsersAsync()
	{
		await _userRepository.DeleteAllUsersAsync();
	}

	public async Task DeleteUserAsync(User user)
	{
		await _userRepository.DeleteAsync(user);
	}

	public async Task<User> AddUserAsync(User user)
	{
		// 1. Проверка возраста
		if (user.Age < 0)
		{
			throw new InvalidOperationException("Пользователь должен быть старше 0 лет.");
		}

		// 2. Проверка уникальности email
		var existingUsers = await _userRepository.GetAllAsync();
		if (existingUsers.Any(u => u.Email == user.Email))
		{
			throw new InvalidOperationException("Пользователь с таким email уже существует.");
		}

		// 3. Генерация нового Id
		user.SetId(Guid.NewGuid());

		// 4. Добавление в репозиторий
		await _userRepository.AddAsync(user);

		return user;
	}

	public async Task AddUsersInTransactionAsync(IEnumerable<User> users, CancellationToken cancellationToken = default)
	{
		await using var transaction = await _userRepository.BeginTransactionAsync();

		try
		{
			foreach (var user in users)
			{
				if (user.Age < 18)
					throw new InvalidOperationException($"Пользователь {user.Name} должен быть старше 18 лет.");

				var existingUsers = await _userRepository.GetAllAsync(cancellationToken);
				if (existingUsers.Any(u => u.Email == user.Email))
					throw new InvalidOperationException($"Пользователь с email {user.Email} уже существует.");

				user.SetId(Guid.NewGuid());
				await _userRepository.AddAsync(user);
			}

			await _userRepository.SaveChangesAsync();
			await transaction.CommitAsync(cancellationToken);
		}
		catch
		{
			await transaction.RollbackAsync(cancellationToken);
			throw;
		}
	}

	public async Task<IEnumerable<User>> SearchUsersAsync(string? name,
	                                                      string? email,
	                                                      int? age,
	                                                      CancellationToken cancellationToken)
	{
		var query = _userRepository.Query();

		if (!string.IsNullOrEmpty(name))
			query = query.Where(u => u.Name.StartsWith(name));

		if (!string.IsNullOrEmpty(email))
			query = query.Where(u => u.Email.StartsWith(email));

		if (age.HasValue)
			query = query.Where(u => u.Age == age);

		return await query.ToListAsync(cancellationToken);
	}

	public async Task<IEnumerable<User>> FilterUsersAsync(string? name, string? email, int? minAge, int? maxAge)
	{
		var query = _userRepository.Query();

		if (!string.IsNullOrEmpty(name))
			query = query.Where(u => u.Name.Contains(name));

		if (!string.IsNullOrEmpty(email))
			query = query.Where(u => u.Email.Contains(email));

		if (minAge.HasValue)
			query = query.Where(u => u.Age >= minAge.Value);

		if (maxAge.HasValue)
			query = query.Where(u => u.Age == maxAge.Value);

		return await query.ToListAsync();
	}

	public async Task<IEnumerable<User>> GetUsersAfterIdAsync(Guid? lastUserId, int pageSize)
	{
		var query = _userRepository.Query();

		if (lastUserId.HasValue)
		{
			query = query.Where(u => u.Id > lastUserId.Value);
		}

		return await query.OrderBy(u => u.Id).Take(pageSize).ToListAsync();
	}

#endregion

#region Pagination

	public async Task<(IEnumerable<User> Users, int TotalCount)> GetPagedUsersAsync(int page, int pageSize)
	{
		var users = _userRepository.Query();
		var totalCount = await users.CountAsync();

		var sortUsers = await users.OrderBy(u => u.Name).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

		return (sortUsers, totalCount);
	}

	public async Task<IEnumerable<User>> GetUsersPagedAsync(UserQueryParams queryParams)
	{
		var query = _userRepository.Query();

		// Сортировка
		if (!string.IsNullOrEmpty(queryParams.SortBy))
		{
			query = queryParams.Descending
				        ? query.OrderByDescending(e => EF.Property<object>(e, queryParams.SortBy))
				        : query.OrderBy(e => EF.Property<object>(e, queryParams.SortBy));
		}

		// Пагинация
		query = query.Skip((queryParams.PageNumber - 1) * queryParams.PageSize).Take(queryParams.PageSize);

		return await query.ToListAsync();
	}

#endregion
}