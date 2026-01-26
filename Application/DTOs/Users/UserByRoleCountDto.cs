namespace AspTelegramBot.Application.DTOs.Users;

/// <summary>
/// DTO, содержащий имя пользователя и количество назначенных ему ролей.
/// </summary>
public class UserByRoleCountDto
{
	public string Name { get; set; } = null!;
	public int RolesCount { get; set; }
}

/// <summary>
/// DTO, содержащий имя пользователя и список названий его ролей.
/// </summary>
public class UserByRoleRolesNameDto
{
	public string Name { get; set; } = null!;
	public List<string> RolesName { get; set; } = new();
}