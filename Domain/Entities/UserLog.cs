namespace AspTelegramBot.Domain.Entities;

/// <summary>
/// Представляет сущность "Лог действий пользователя".
/// </summary>
public class UserLog
{
	public Guid Id { get; set; } = Guid.NewGuid();
	public string? UserEmail { get; set; }
	public string? Action { get; set; }
	public string? Path { get; set; }
	public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
