using AspTelegramBot.Application.DTOs.Users;
using AspTelegramBot.Domain.Entities;

namespace AspTelegramBot.Application.Interfaces.ForUser;

/// <summary>
/// Интерфейс для управления пользователями.
/// </summary>
public interface IUserService
{
	/// <summary>
	/// Асинхронно извлекает всех пользователей.
	/// </summary>
	/// <param name="cancellationToken">Токен для отслеживания запросов на отмену.</param>
	/// <returns>Коллекция UserResponseDto или null, если пользователи не найдены.</return>
	Task<IEnumerable<UserResponseDto>?> GetAllUsersAsync(CancellationToken cancellationToken);

	/// <summary>
	/// Возвращает строку соединения с базой данных
	/// </summary>
	/// <returns>Строка соединения</returns>
	string GetConnectionInfo();

	/// <summary>
	/// Получить пользователя по идентификатору асинхронно
	/// </summary>
	/// <param name="id">Идентификатор пользователя</param>
	/// <returns>Возвращает объект User или null, если пользователь не найден</returns>
	Task<User?> GetUserByIdAsync(Guid id);

	/// <summary>
	/// Асинхронно извлекает пользователя по указанному идентификатору.
	/// </summary>
	/// <param name="id">Уникальный идентификатор пользователя (Telegram ID).</param>
	/// <returns>Экземпляр объекта User или null, если пользователь не найден.</returns>
	Task<User?> GetUserByIdAsync(long id);

	/// <summary>
	/// Асинхронно добавляет нового пользователя в систему.
	/// </summary>
	/// <param name="user">Экземпляр пользователя, который будет добавлен.</param>
	/// <returns>Добавленный пользователь с назначенными свойствами, такими как идентификатор.</returns>
	Task<User> AddUserAsync(User user);

	/// <summary>
	/// Асинхронно обновляет информацию о пользователе.
	/// </summary>
	/// <param name="user">Объект пользователя с обновленными данными.</param>
	/// <returns>Обновленный объект User.</returns>
	Task<User> UpdateUserAsync(User user);

	/// <summary>
	/// Асинхронно удаляет указанного пользователя.
	/// </summary>
	/// <param name="user">Объект пользователя, который необходимо удалить.</param>
	/// <returns>Задача, представляющая асинхронную операцию удаления.</returns>
	Task DeleteUserAsync(User user);

	/// <summary>
	/// Асинхронно удаляет всех пользователей из системы.
	/// </summary>
	/// <returns>Задача, представляющая асинхронную операцию удаления всех пользователей.</returns>
	Task DeleteAllUsersAsync();

	/// <summary>
	/// Асинхронно фильтрует пользователей по указанным параметрам.
	/// </summary>
	/// <param name="name">Имя пользователя для фильтрации или null.</param>
	/// <param name="email">Email пользователя для фильтрации или null.</param>
	/// <param name="minAge">Минимальный возраст пользователя для фильтрации или null.</param>
	/// <param name="maxAge">Максимальный возраст пользователя для фильтрации или null.</param>
	/// <returns>Коллекция объектов User, соответствующих заданным критериям.</returns>
	Task<IEnumerable<User>> FilterUsersAsync(string? name, string? email, int? minAge, int? maxAge);

	/// <summary>
	/// Асинхронно извлекает пользователей с постраничной разбивкой.
	/// </summary>
	/// <param name="page">Номер страницы для извлечения данных.</param>
	/// <param name="pageSize">Количество пользователей на одной странице.</param>
	/// <returns>Кортеж, содержащий коллекцию пользователей и общее количество пользователей.</returns>
	Task<(IEnumerable<User> Users, int TotalCount)> GetPagedUsersAsync(int page, int pageSize);

	/// <summary>
	/// Асинхронно добавляет пользователей в пределах транзакции.
	/// </summary>
	/// <param name="users">Список пользователей для добавления.</param>
	/// <param name="cancellationToken">Токен для отслеживания запросов на отмену.</param>
	/// <returns>Задача, представляющая асинхронную операцию.</returns>
	Task AddUsersInTransactionAsync(IEnumerable<User> users, CancellationToken cancellationToken);

	/// <summary>
	/// Асинхронно извлекает пользователей, идентификаторы которых идут после указанного.
	/// </summary>
	/// <param name="lastUserId">Идентификатор последнего пользователя, после которого нужно начать выборку. Может быть null.</param>
	/// <param name="pageSize">Количество пользователей, которые нужно извлечь.</param>
	/// <returns>Коллекция объектов User.</returns>
	Task<IEnumerable<User>> GetUsersAfterIdAsync(Guid? lastUserId, int pageSize);

	/// <summary>
	/// Асинхронно обновляет пользователя с учетом проверки конфликтов конкурентности.
	/// </summary>
	/// <param name="user">Объект пользователя, содержащий обновляемые данные.</param>
	/// <returns>Обновленный объект пользователя.</returns>
	Task<User> UpdateUserWithConcurrencyCheckAsync(User user);

	/// <summary>
	/// Асинхронно извлекает пользователей с поддержкой пагинации.
	/// </summary>
	/// <param name="queryParams">Параметры запроса, включающие номер страницы, размер страницы, сортировку и порядок.</param>
	/// <returns>Коллекция объектов User, соответствующих критериям пагинации.</returns>
	Task<IEnumerable<User>> GetUsersPagedAsync(UserQueryParams queryParams);

	/// <summary>
	/// Асинхронно выполняет поиск пользователей по указанным параметрам.
	/// </summary>
	/// <param name="name">Имя пользователя, используемое для фильтрации. Может быть null.</param>
	/// <param name="email">Email пользователя, используемый для фильтрации. Может быть null.</param>
	/// <param name="age">Возраст пользователя, используемый для фильтрации. Может быть null.</param>
	/// <param name="cancellationToken">Токен для отслеживания запросов на отмену.</param>
	/// <returns>Коллекция пользователей, соответствующих заданным параметрам.</returns>
	Task<IEnumerable<User>> SearchUsersAsync(string? name, string? email, int? age, CancellationToken cancellationToken);
}