namespace AspTelegramBot.Domain.Entities;

/// <summary>
/// Представляет сущность фразы бота, содержащую триггер, категорию и текст ответа.
/// </summary>
public class BotPhrase
{
	public int Id { get; set; }
	public string Category { get; set; } = null!;
	public string TriggerText { get; set; } = null!;
	public string ResponseText { get; set; } = null!;
}