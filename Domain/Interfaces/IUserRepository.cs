using AspTelegramBot.Domain.Entities;
using Microsoft.EntityFrameworkCore.Storage;

namespace AspTelegramBot.Domain.Interfaces;

/// <summary>
/// Интерфейс для работы с сущностями пользователя в хранилище.
/// </summary>
public interface IUserRepository
{
	/// <summary>
	/// Асинхронно получает все сущности пользователей из хранилища.
	/// </summary>
	/// <param name="cancellationToken">Токен, используемый для обработки отмены операции.</param>
	/// <returns>Коллекция всех пользователей.</returns>
	Task<IEnumerable<User>> GetAllAsync(CancellationToken cancellationToken = default);

	/// <summary>
	/// Асинхронно добавляет нового пользователя в хранилище.
	/// </summary>
	/// <param name="user">Экземпляр пользователя, который необходимо добавить.</param>
	/// <returns>Задача, представляющая добавление пользователя в хранилище.</returns>
	Task AddAsync(User user);

	/// <summary>
	/// Асинхронно получает сущность пользователя по указанному идентификатору.
	/// </summary>
	/// <param name="id">Уникальный идентификатор пользователя.</param>
	/// <returns>Сущность пользователя или null, если пользователь не найден.</returns>
	Task<User?> GetByIdAsync(Guid id);

	/// <summary>
	/// Асинхронно получает пользователя по его идентификатору Telegram.
	/// </summary>
	/// <param name="id">Идентификатор Telegram пользователя.</param>
	/// <returns>Найденный пользователь или null, если пользователь не найден.</returns>
	Task<User?> GetByIdAsync(long id);

	/// <summary>
	/// Асинхронно обновляет данные пользователя в хранилище.
	/// </summary>
	/// <param name="user">Объект пользователя, содержащий обновленные данные.</param>
	/// <returns>Задача, представляющая асинхронную операцию обновления.</returns>
	Task UpdateAsync(User user);

	/// <summary>
	/// Асинхронно удаляет пользователя из хранилища.
	/// </summary>
	/// <param name="user">Экземпляр пользователя, который должен быть удален из хранилища.</param>
	/// <returns>Задача, представляющая выполнение операции удаления.</returns>
	Task DeleteAsync(User user);

	/// <summary>
	/// Асинхронно удаляет всех пользователей из хранилища.
	/// </summary>
	/// <returns>Задача, представляющая операцию удаления всех пользователей.</returns>
	Task DeleteAllUsersAsync();

	/// <summary>
	/// Асинхронно начинает новую транзакцию в базе данных.
	/// </summary>
	/// <returns>Объект транзакции для управления выполнением операций в рамках одной транзакции.</returns>
	Task<IDbContextTransaction> BeginTransactionAsync();

	/// <summary>
	/// Асинхронно сохраняет изменения в хранилище.
	/// </summary>
	/// <returns>Задача, представляющая операцию сохранения изменений.</returns>
	Task SaveChangesAsync();

	/// <summary>
	/// Асинхронно добавляет сущность токена обновления в хранилище.
	/// </summary>
	/// <param name="refreshToken">Экземпляр токена обновления, который необходимо добавить.</param>
	/// <returns>Задача, представляющая добавление токена обновления в хранилище.</returns>
	Task AddRefreshTokensAsync(RefreshToken refreshTokens);

	/// <summary>
	/// Получает запрос для выборки пользователей из хранилища.
	/// </summary>
	/// <returns>Объект запроса для выборки пользователей.</returns>
	IQueryable<User> Query();

	/// <summary>
	/// Возвращает запрос для получения токенов обновления из хранилища.
	/// </summary>
	/// <returns>Запрос, представляющий токены обновления.</returns>
	IQueryable<RefreshToken> QueryTokens();

	/// <summary>
	/// Асинхронно обновляет сущность токена обновления в хранилище.
	/// </summary>
	/// <param name="refreshToken">Экземпляр токена обновления, который необходимо обновить.</param>
	/// <returns>Задача, представляющая обновление токена в хранилище.</returns>
	Task UpdateToken(RefreshToken refreshToken);
}