using AspTelegramBot.Application.Filters;
using AspTelegramBot.Application.Interfaces.ForHandler;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace AspTelegramBot.Application.Handlers;

public class StickerHandler : IUpdateHandler
{
	private readonly TelegramBotClient _botClient;
	private readonly TelegramMessageFilter _telegramMessageFilter;

	public ChatAction ChatAction => ChatAction.ChooseSticker;

	public StickerHandler(TelegramMessageFilter telegramMessageFilter, TelegramBotClient botClient)
	{
		_telegramMessageFilter = telegramMessageFilter;
		_botClient = botClient;
	}

	public async Task<bool> HandleAsync(Update update, CancellationToken ct)
	{
		if (update.Message.Type != MessageType.Sticker)
			return false;

		await _botClient.SendChatActionAsync(update.Message.Chat.Id, ChatAction, cancellationToken: ct);

		_telegramMessageFilter.Enqueue(update.Message.Chat.Id,
		                               $"FileId стикера:\n{update.Message.Sticker.FileId}",
		                               ct: ct);

		return true;
	}
}