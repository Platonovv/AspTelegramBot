namespace AspTelegramBot.Domain.Entities;

public class AudioFile
{
	public int Id { get; set; }

	public string Key { get; set; } = null!;

	public string FileId { get; set; } = null!;

	public string FileHash { get; set; } = null!;

	public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}