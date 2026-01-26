using AspTelegramBot.Application.Interfaces;
using AspTelegramBot.Domain.Entities;
using AspTelegramBot.Domain.Interfaces;

namespace AspTelegramBot.Application.Services;

/// <summary>
/// Предоставляет услуги, связанные с управлением ролями в приложении.
/// </summary>
public class RoleService : IRoleService
{
	private readonly IRoleRepository _roleRepository;

	public RoleService(IRoleRepository roleRepository)
	{
		_roleRepository = roleRepository;
	}

	public async Task<Role?> GetByIdAsync(Guid id) => await _roleRepository.GetByIdAsync(id);

	public async Task<IEnumerable<Role?>> GetAllAsync() => await _roleRepository.GetAllAsync();

	public async Task<Role?> AddRoleAsync(Role? role)
	{
		await _roleRepository.AddAsync(role);
		return role;
	}

	public async Task<Role?> UpdateRoleAsync(Role? role)
	{
		await _roleRepository.UpdateAsync(role);
		return role;
	}

	public async Task DeleteRoleAsync(Role? role)
	{
		await _roleRepository.DeleteAsync(role);
	}

	public async Task<Role?> GetRoleByNameAsync(string roleName)
	{
		return await _roleRepository.GetByNameAsync(roleName);
	}
}