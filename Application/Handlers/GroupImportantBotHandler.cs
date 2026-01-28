using AspTelegramBot.Application.Filters;
using AspTelegramBot.Application.Interfaces.ForHandler;
using AspTelegramBot.Infrastructure.Repositories;
using Telegram.Bot.Types;

namespace AspTelegramBot.Application.Handlers;

/// <summary>
/// Обрабатывает важные сообщения в группах на основе ключевых слов.
/// </summary>
public class GroupImportantBotHandler : IUpdateHandler
{
	private readonly BotPhrasesRepository _repository;
	private readonly TelegramMessageFilter _telegramMessageFilter;

	public GroupImportantBotHandler(BotPhrasesRepository repository, TelegramMessageFilter telegramMessageFilter)
	{
		_repository = repository;
		_telegramMessageFilter = telegramMessageFilter;
	}

	public async Task<bool> HandleAsync(Update update, CancellationToken ct)
	{
		if (update.Message == null)
			return false;

		var keywords = await _repository.GetGroupKeywordsAsync();
		var text = update.Message.Text;

		foreach (var key in keywords.Keys)
		{
			if (text != null && text.Contains(key, StringComparison.OrdinalIgnoreCase))
			{
				_telegramMessageFilter.Enqueue(update.Message.Chat.Id, keywords[key], ct: ct);
				return true;
			}
		}

		return false;
	}
}