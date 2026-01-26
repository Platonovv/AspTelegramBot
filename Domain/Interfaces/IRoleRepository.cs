using AspTelegramBot.Domain.Entities;

namespace AspTelegramBot.Domain.Interfaces;

/// <summary>
/// Интерфейс для работы с хранилищем ролей.
/// </summary>
public interface IRoleRepository
{
	/// <summary>
	/// Получает все роли из хранилища.
	/// </summary>
	/// <returns>
	/// Коллекция объектов типа Role или null.
	/// </returns>
	Task<IEnumerable<Role?>> GetAllAsync();

	/// <summary>
	/// Асинхронно получает роль из хранилища по идентификатору.
	/// </summary>
	/// <param name="id">
	/// Уникальный идентификатор роли.
	/// </param>
	/// <returns>
	/// Объект типа Role или null, если роль не найдена.
	/// </returns>
	Task<Role?> GetByIdAsync(Guid id);

	/// <summary>
	/// Асинхронно добавляет новую роль в хранилище.
	/// </summary>
	/// <param name="role">
	/// Объект типа Role, который необходимо добавить в хранилище.
	/// </param>
	/// <returns>
	/// Пустая задача, представляющая результат выполнения операции добавления.
	/// </returns>
	Task AddAsync(Role? role);

	/// <summary>
	/// Асинхронно обновляет существующую роль в хранилище.
	/// </summary>
	/// <param name="role">
	/// Объект типа Role, который необходимо обновить в хранилище.
	/// </param>
	/// <returns>
	/// Пустая задача, представляющая результат выполнения операции обновления.
	/// </returns>
	Task UpdateAsync(Role? role);

	/// <summary>
	/// Асинхронно удаляет указанную роль из хранилища.
	/// </summary>
	/// <param name="role">
	/// Объект типа Role, который необходимо удалить из хранилища.
	/// </param>
	/// <returns>
	/// Пустая задача, представляющая результат выполнения операции удаления.
	/// </returns>
	Task DeleteAsync(Role? role);

	/// <summary>
	/// Асинхронно получает роль из хранилища по имени.
	/// </summary>
	/// <param name="roleName">
	/// Имя роли, которую необходимо получить.
	/// </param>
	/// <returns>
	/// Объект типа Role или null, если роль с указанным именем не найдена.
	/// </returns>
	Task<Role?> GetByNameAsync(string roleName);
}