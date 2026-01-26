using AspTelegramBot.Application.DTOs.Users;
using AspTelegramBot.Domain.Entities;

namespace AspTelegramBot.Application.Interfaces.ForUser;

/// <summary>
/// Интерфейс для управления ролями пользователя.
/// </summary>
public interface IUserRoleService
{
	/// <summary>
	/// Назначает указанные роли заданному пользователю, заменяя любые существующие роли.
	/// </summary>
	/// <param name="user">Пользователь, которому будут назначены роли.</param>
	/// <param name="roles">Коллекция ролей, которые будут назначены пользователю.</param>
	/// <returns>Задача, представляющая асинхронную операцию.</returns>
	Task AssignRolesToUserAsync(User user, IEnumerable<Role> roles);

	/// <summary>
	/// Удаляет все роли, связанные с указанным пользователем.
	/// </summary>
	/// <param name="user">Пользователь, у которого будут удалены все роли.</param>
	/// <returns>Задача, представляющая асинхронную операцию.</returns>
	Task RemoveRolesFromUserAsync(User user);

	/// <summary>
	/// Возвращает коллекцию данных о пользователях с указанием количества их ролей.
	/// </summary>
	/// <returns>Задача, представляющая асинхронную операцию, возвращающую коллекцию объектов типа UserByRoleCountDto.</returns>
	Task<IEnumerable<UserByRoleCountDto>> GetUsersRoleCountsAsync();

	/// <summary>
	/// Возвращает коллекцию данных о пользователях, включающую их имена и списки названий их ролей.
	/// </summary>
	/// <returns>Задача, представляющая асинхронную операцию, возвращающую коллекцию объектов типа UserByRoleRolesNameDto.</returns>
	Task<IEnumerable<UserByRoleRolesNameDto>> GetAllUserRolesFlatAsync();

	/// <summary>
	/// Получает список пользователей, принадлежащих указанной роли.
	/// </summary>
	/// <param name="roleName">Название роли, для которой необходимо получить пользователей.</param>
	/// <returns>Задача, содержащая коллекцию объектов типа UserResponseDto.</returns>
	Task<IEnumerable<UserResponseDto>> GetUsersByRoleAsync(string roleName);
}