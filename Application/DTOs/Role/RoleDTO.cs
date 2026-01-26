namespace AspTelegramBot.Application.DTOs.Role;

/// <summary>
/// DTO для предоставления данных о роли.
/// </summary>
public class RoleDTO
{
	public Guid Id { get; set; }
	public string Name { get; set; } = null!;
}