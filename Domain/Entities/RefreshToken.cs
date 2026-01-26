namespace AspTelegramBot.Domain.Entities;

/// <summary>
/// Представляет токен обновления для аутентификации пользователя.
/// </summary>
public class RefreshToken
{
	public Guid Id { get; set; }
	public string Token { get; set; } = null!;
	public DateTime ExpiresAt { get; set; }
	public bool IsRevoked { get; set; } = false;

	public Guid UserId { get; set; }
	public User User { get; set; } = null!;
}
