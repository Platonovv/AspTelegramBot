using AspTelegramBot.Application.Filters;
using AspTelegramBot.Infrastructure.Repositories;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace AspTelegramBot.Application.Handlers;

/// <summary>
/// Обрабатывает операции, связанные с тегами и их обработкой в сообщениях.
/// </summary>
public class TagHandler
{
	private readonly Random _rnd = new();
	private Dictionary<string, List<string>>? _tags;
	private readonly BotPhrasesRepository _repository;
	private readonly TelegramMessageFilter _telegramMessageFilter;

	public TagHandler(BotPhrasesRepository repository, TelegramMessageFilter telegramMessageFilter)
	{
		_repository = repository;
		_telegramMessageFilter = telegramMessageFilter;
	}

	public async Task<IEnumerable<string>> GetAllTags()
	{
		var tagsDesc = await _repository.GetTagsAsync();
		return tagsDesc.Keys;
	}

	public async Task<bool> HandleTagAsync(Update update, string messageText, CancellationToken ct)
	{
		_tags = await _repository.GetTagsAsync();

		foreach (var (keyword, responses) in _tags)
		{
			if (!messageText.Contains(keyword, StringComparison.OrdinalIgnoreCase))
				continue;

			var parts = messageText.Split(' ', StringSplitOptions.RemoveEmptyEntries);
			if (parts.Length < 2)
			{
				_telegramMessageFilter.Enqueue(update.Message.Chat.Id,
				                              $"Используй так: {keyword} @никнейм @Ivan_Kalimistz_Kallen_bot",
				                              ct: ct);
				return true;
			}

			var targetUsername = parts[1];
			var responseText = responses[_rnd.Next(responses.Count)].Replace("{username}", targetUsername);

			_telegramMessageFilter.Enqueue(update.Message.Chat.Id, responseText, ct: ct);
			return true;
		}

		return false;
	}
}