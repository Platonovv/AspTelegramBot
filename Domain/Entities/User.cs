using System.ComponentModel.DataAnnotations;

namespace AspTelegramBot.Domain.Entities;

/// <summary>
/// Представляет сущность "Пользователь".
/// </summary>
public class User
{
	public Guid Id { get; set; }               // уникальный идентификатор
	public string Name { get; set; } = null!;  // имя пользователя
	public string Email { get; set; } = null!; // email пользователя
	public int Age { get; set; }               // возраст
	public long TelegramID { get; set; }               // Telegram ID
	public void SetId(Guid id) => Id = id;
	public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

	public string PasswordHash { get; set; } = null!; // Новый: хэш пароля
	public ICollection<Role> Roles { get; set; } = new List<Role>();

	[Timestamp]
	public byte[]? RowVersion { get; set; } = null!;

	// Email

	public bool EmailConfirmed { get; set; } = false;
	public string? EmailConfirmationTokenHash { get; set; } // хеш токена
	public DateTime? EmailConfirmationExpiresAt { get; set; }
}