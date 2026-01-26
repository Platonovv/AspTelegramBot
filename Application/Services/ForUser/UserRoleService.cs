using AspTelegramBot.Application.DTOs.Role;
using AspTelegramBot.Application.DTOs.Users;
using AspTelegramBot.Application.Interfaces.ForUser;
using AspTelegramBot.Domain.Entities;
using AspTelegramBot.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AspTelegramBot.Application.Services.ForUser;

/// <summary>
/// Служба, отвечающая за управление ролями пользователей в приложении.
/// </summary>
public class UserRoleService : IUserRoleService
{
	private readonly IUserRepository _userRepository;

	public UserRoleService(IUserRepository userRepository)
	{
		_userRepository = userRepository;
	}

	public async Task AssignRolesToUserAsync(User user, IEnumerable<Role> roles)
	{
		user.Roles.Clear();
		foreach (var role in roles)
			user.Roles.Add(role);

		await _userRepository.UpdateAsync(user);
	}

	public async Task RemoveRolesFromUserAsync(User user)
	{
		user.Roles.Clear();
		await _userRepository.UpdateAsync(user);
	}

	public async Task<IEnumerable<UserByRoleCountDto>> GetUsersRoleCountsAsync()
	{
		var usersByRoleCountDto = await _userRepository.Query()
		                                               .Select(u => new UserByRoleCountDto
		                                               {
			                                               Name = u.Name, RolesCount = u.Roles.Count
		                                               })
		                                               .ToListAsync();

		return usersByRoleCountDto;
	}

	public async Task<IEnumerable<UserByRoleRolesNameDto>> GetAllUserRolesFlatAsync()
	{
		var usersByRoleRolesNameDto = await _userRepository.Query()
		                                                   .Include(u => u.Roles)
		                                                   .AsNoTracking()
		                                                   .Select(u => new UserByRoleRolesNameDto
		                                                   {
			                                                   Name = u.Name,
			                                                   RolesName = u.Roles
			                                                                .Select(r => r.Name)
			                                                                .ToList()
		                                                   })
		                                                   .ToListAsync();

		return usersByRoleRolesNameDto;
	}

	public async Task<IEnumerable<UserResponseDto>> GetUsersByRoleAsync(string roleName)
	{
		return await _userRepository.Query()
		                            .Include(u => u.Roles)
		                            .Where(x => x.Roles.Any(r => r.Name == roleName))
		                            .Select(u => new UserResponseDto
		                            {
			                            Id = u.Id,
			                            Name = u.Name,
			                            Email = u.Email,
			                            Age = u.Age,
			                            TelegramID = u.TelegramID,
			                            Roles = u.Roles
			                                     .Select(r => new RoleDTO {Id = r.Id, Name = r.Name})
			                                     .ToList()
		                            })
		                            .ToListAsync();
	}
}