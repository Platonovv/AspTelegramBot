using AspTelegramBot.Application.Filters;
using AspTelegramBot.Infrastructure.Repositories;
using Telegram.Bot.Types;

namespace AspTelegramBot.Application.Handlers;

/// <summary>
/// Класс для обработки ключевых слов, хранящихся в базе данных.
/// </summary>
public class KeywordHandler
{
	private readonly BotPhrasesRepository _repository;
	private readonly TelegramMessageFilter _telegramMessageFilter;

	public KeywordHandler(BotPhrasesRepository repository, TelegramMessageFilter telegramMessageFilter)
	{
		_repository = repository;
		_telegramMessageFilter = telegramMessageFilter;
	}

	public async Task<IEnumerable<string>> GetAllKeywordsAsync()
	{
		var keywordItems = await _repository.GetKeywordRegexAsync();
		return keywordItems.Select(x => x.key).ToList();
	}

	public async Task<bool> HandleKeyword(Update update, string messageText, CancellationToken ct)
	{
		var keywords = await _repository.GetKeywordRegexAsync();

		foreach (var (regex, _, response) in keywords)
		{
			if (!regex.IsMatch(messageText))
				continue;

			_telegramMessageFilter.Enqueue(update.Message?.Chat.Id, response, ct: ct);
			return true;
		}

		return false;
	}
}