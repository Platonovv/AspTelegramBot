using AspTelegramBot.Application.Filters;
using AspTelegramBot.Application.Interfaces.ForHandler;
using AspTelegramBot.Infrastructure.Repositories;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace AspTelegramBot.Application.Handlers;

/// <summary>
/// Обрабатывает операции, связанные с тегами и их обработкой в сообщениях.
/// </summary>
public class TagHandler : IUpdateHandler
{
	private readonly TelegramBotClient _botClient;
	private readonly BotPhrasesRepository _repository;
	private readonly TelegramMessageFilter _telegramMessageFilter;
	private readonly Random _rnd = new();

	public ChatAction ChatAction => ChatAction.Typing;

	public TagHandler(BotPhrasesRepository repository,
	                  TelegramMessageFilter telegramMessageFilter,
	                  TelegramBotClient botClient)
	{
		_repository = repository;
		_telegramMessageFilter = telegramMessageFilter;
		_botClient = botClient;
	}

	public async Task<bool> HandleAsync(Update update, CancellationToken ct)
	{
		if (update.Message == null)
			return false;

		var tags = await _repository.GetTagsAsync();
		var text = update.Message.Text;

		foreach (var (keyword, responses) in tags)
		{
			if (!text.Contains(keyword, StringComparison.OrdinalIgnoreCase))
				continue;

			await _botClient.SendChatActionAsync(update.Message.Chat.Id, ChatAction, cancellationToken: ct);

			var parts = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
			if (parts.Length < 2)
			{
				_telegramMessageFilter.Enqueue(update.Message.Chat.Id,
				                               $"Используй так: {keyword} @никнейм @bot_name",
				                               ct: ct);
				return true;
			}

			var targetUsername = parts[1];
			var response = responses[_rnd.Next(responses.Count)].Replace("{username}", targetUsername);
			_telegramMessageFilter.Enqueue(update.Message.Chat.Id, response, ct: ct);
			return true;
		}

		return false;
	}

	public async Task<IEnumerable<string>> GetAllTags()
	{
		var tags = await _repository.GetTagsAsync();
		return tags.Keys;
	}
}