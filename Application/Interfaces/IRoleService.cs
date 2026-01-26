using AspTelegramBot.Domain.Entities;

namespace AspTelegramBot.Application.Interfaces;

/// <summary>
/// Интерфейс для управления данными сущности «Роль».
/// </summary>
public interface IRoleService
{
	/// <summary>
	/// Асинхронно извлекает сущность «Роль» по её уникальному идентификатору.
	/// </summary>
	/// <param name="id">Уникальный идентификатор извлекаемой роли.</param>
	/// <returns>Задача, представляющая асинхронную операцию, содержащая сущность «Роль», если найдена, в противном случае возвращает null.</returns>
	Task<Role?> GetByIdAsync(Guid id);

	/// <summary>
	/// Асинхронно извлекает все сущности «Роль» из хранилища.
	/// </summary>
	/// <returns>Задача, представляющая асинхронную операцию, содержащая коллекцию сущностей «Роль».</returns>
	Task<IEnumerable<Role?>> GetAllAsync();

	/// <summary>
	/// Асинхронно добавляет новую сущность «Роль» в хранилище.
	/// </summary>
	/// <param name="role">Сущность «Роль», которую необходимо добавить.</param>
	/// <returns>Задача, представляющая асинхронную операцию, содержащая добавленную сущность «Роль».</returns>
	Task<Role?> AddRoleAsync(Role? role);

	/// <summary>
	/// Асинхронно обновляет существующую сущность «Роль» в хранилище.
	/// </summary>
	/// <param name="role">Сущность «Роль», которую необходимо обновить.</param>
	/// <returns>Задача, представляющая асинхронную операцию, содержащая обновленную сущность «Роль».</returns>
	Task<Role?> UpdateRoleAsync(Role? role);

	/// <summary>
	/// Асинхронно удаляет указанную сущность «Роль» из хранилища.
	/// </summary>
	/// <param name="role">Сущность «Роль», которую необходимо удалить.</param>
	/// <returns>Задача, представляющая асинхронную операцию удаления сущности.</returns>
	Task DeleteRoleAsync(Role? role);

	/// <summary>
	/// Асинхронно извлекает сущность «Роль» на основе её уникального имени.
	/// </summary>
	/// <param name="roleName">Имя извлекаемой роли.</param>
	/// <returns>Задача, представляющая асинхронную операцию, содержащая сущность «Роль», если найдена; в противном случае возвращает null.</returns>
	Task<Role?> GetRoleByNameAsync(string roleName);
}