using AspTelegramBot.Application.DTOs.Role;

namespace AspTelegramBot.Application.DTOs.Users;

/// <summary>
/// DTO для представления ответа с данными о пользователе.
/// </summary>
public class UserResponseDto
{
	public Guid Id { get; set; }
	public string Name { get; set; } = null!;
	public string Email { get; set; } = null!;
	public int Age { get; set; }
	public long TelegramID { get; set; } // Telegram ID
	public int RolesCount { get; set; }
	public DateTime CreatedAt { get; set; }
	public List<RoleDTO> Roles { get; set; } = new();
}