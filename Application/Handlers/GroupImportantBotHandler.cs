using AspTelegramBot.Application.Filters;
using AspTelegramBot.Infrastructure.Repositories;
using Telegram.Bot.Types;

namespace AspTelegramBot.Application.Handlers;

/// <summary>
/// Обрабатывает важные сообщения в группах на основе ключевых слов.
/// </summary>
public class GroupImportantBotHandler
{
	private readonly BotPhrasesRepository _rep;
	private Dictionary<string, string>? _groupKeywords;
	private readonly TelegramMessageFilter _telegramMessageFilter;

	public GroupImportantBotHandler(BotPhrasesRepository rep, TelegramMessageFilter telegramMessageFilter)
	{
		_rep = rep;
		_telegramMessageFilter = telegramMessageFilter;
	}

	public async Task HandleKeyword(Update update, string messageText, CancellationToken ct)
	{
		_groupKeywords = await _rep.GetGroupKeywordsAsync();

		foreach (var key in _groupKeywords.Keys)
		{
			if (!messageText.Contains(key, StringComparison.OrdinalIgnoreCase))
				continue;

			_telegramMessageFilter.Enqueue(update.Message.Chat.Id, _groupKeywords[key], ct: ct);

			return;
		}
	}
}