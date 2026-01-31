using AspTelegramBot.Application.Filters;
using AspTelegramBot.Application.Interfaces.ForHandler;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace AspTelegramBot.Application.Handlers;

public class StickerHandler : IUpdateHandler
{
	private readonly TelegramMessageFilter _telegramMessageFilter;

	public StickerHandler(TelegramMessageFilter telegramMessageFilter)
	{
		_telegramMessageFilter = telegramMessageFilter;
	}

	public async Task<bool> HandleAsync(Update update, CancellationToken ct)
	{
		if (update.Message.Type != MessageType.Sticker)
			return false;

		_telegramMessageFilter.Enqueue(update.Message.Chat.Id,
		                               $"FileId стикера:\n{update.Message.Sticker.FileId}",
		                               ct: ct);

		return true;
	}
}