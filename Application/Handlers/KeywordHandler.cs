using AspTelegramBot.Application.Filters;
using AspTelegramBot.Application.Interfaces.ForHandler;
using AspTelegramBot.Infrastructure.Repositories;
using Telegram.Bot.Types;

namespace AspTelegramBot.Application.Handlers;

/// <summary>
/// Класс для обработки ключевых слов, хранящихся в базе данных.
/// </summary>
public class KeywordHandler : IUpdateHandler
{
	private readonly BotPhrasesRepository _repository;
	private readonly TelegramMessageFilter _telegramMessageFilter;

	public KeywordHandler(BotPhrasesRepository repository, TelegramMessageFilter telegramMessageFilter)
	{
		_repository = repository;
		_telegramMessageFilter = telegramMessageFilter;
	}

	public async Task<bool> HandleAsync(Update update, CancellationToken ct)
	{
		if (update.Message == null)
			return false;

		var keywords = await _repository.GetKeywordRegexAsync();
		foreach (var (regex, _, response) in keywords)
		{
			if (!regex.IsMatch(update.Message.Text))
				continue;

			_telegramMessageFilter.Enqueue(update.Message.Chat.Id, response, ct: ct);
			return true;
		}

		return false;
	}

	public async Task<IEnumerable<string>> GetAllKeywordsAsync()
	{
		var items = await _repository.GetKeywordRegexAsync();
		return items.Select(x => x.key);
	}
}