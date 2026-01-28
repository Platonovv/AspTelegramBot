using Telegram.Bot.Types;

namespace AspTelegramBot.Application.Interfaces.ForHandler;

public interface IUpdateHandler
{
	/// <summary>
	/// Обрабатывает апдейт. Возвращает true, если апдейт обработан и дальнейшие обработчики не нужны.
	/// </summary>
	Task<bool> HandleAsync(Update update, CancellationToken ct);
}