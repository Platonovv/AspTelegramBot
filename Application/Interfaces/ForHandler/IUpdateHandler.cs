using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace AspTelegramBot.Application.Interfaces.ForHandler;

public interface IUpdateHandler
{
	/// <summary>
	/// Обрабатывает апдейт. Возвращает true, если апдейт обработан и дальнейшие обработчики не нужны.
	/// </summary>
	Task<bool> HandleAsync(Update update, CancellationToken ct);

	/// <summary>
	/// Представляет собой выполняемое действие в чате
	/// Указывает на текущую активность (например, набор текста, загрузка, запись и т. д.)
	/// в чате, связанном с обработчиком.
	/// Может использоваться для сигнализации статуса выполняемых действий.
	/// </summary>
	ChatAction ChatAction { get; }
}