using AspTelegramBot.Application.Filters;
using AspTelegramBot.Application.Handlers;
using AspTelegramBot.Application.Services.Bot;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

/// <summary>
/// Класс для обработки различных команд бота.
/// </summary>
public class CommandHandler
{
	private readonly TagHandler _tagHandler;
	private readonly TelegramBotClient _botClient;
	private readonly KeywordHandler _keywordHandler;
	private readonly ILogger<TelegramBotService> _logger;
	private readonly TelegramMessageFilter _telegramMessageFilter;

	private readonly Dictionary<string, Func<Update, CancellationToken, Task>> _commands;

	public CommandHandler(TelegramBotClient botClient,
	                      ILogger<TelegramBotService> logger,
	                      KeywordHandler keywordHandler,
	                      TagHandler tagHandler,
	                      TelegramMessageFilter telegramMessageFilter)
	{
		_logger = logger;
		_botClient = botClient;
		_tagHandler = tagHandler;
		_keywordHandler = keywordHandler;
		_telegramMessageFilter = telegramMessageFilter;

		_commands = new Dictionary<string, Func<Update, CancellationToken, Task>>(StringComparer.OrdinalIgnoreCase)
		{
			{"/commands", HandleCommandsAsync},
			{"/start", HandleStartAsync},
			{"/hello", HandleHelloAsync},
			{"/game", HandleGameAsync},
			{"/quiz", HandleQuizAsync},
			{
				"/misha",
				HandleStickerAsync("CAACAgIAAxkBAAM3aQyJ-u2Z13Z73C39Yvw5dpRaM-oAAuyGAAKs9ElIiV1v7bo2cLY2BA")
			},
			{
				"/vanya",
				HandleStickerAsync("CAACAgIAAxkBAAM5aQyKAvVEobWnRk3ta9cf4NcHjzkAAll_AAJXF0BISg4y0sxlb9I2BA")
			},
			{
				"/fedya",
				HandleStickerAsync("CAACAgIAAxkBAAM7aQyKC0GtkI-cKC9SKJ0ktPfx_tIAAtKFAAJg-kBI4Jr_ecEGYvk2BA")
			},
			{
				"/grystno",
				HandleStickerAsync("CAACAgIAAxkBAAM_aQyKQ44sLT1MAAFaUgZMqOYBCIBiAAKePwACKxrBSQ9tMdUadxVKNgQ")
			},
			{
				"/dima",
				HandleStickerAsync("CAACAgIAAxkBAAIB5mkNxtB8NI4IlrFzT4SJ6WGurMujAAJ7jwACgVFoSBFwCUl_EhndNgQ")
			},
			{
				"/banda",
				HandleStickerAsync("CAACAgIAAxkBAAIB6GkNx5piapWOhSHoRuu5psPWPl6zAAJweAACTKNxSLvmKsNWvEm_NgQ")
			},
		};
	}

	public async Task<bool> HandleCommand(Update update, string messageText, CancellationToken ct)
	{
		if (!_commands.TryGetValue(messageText, out var handler))
			return false;

		await handler(update, ct).WaitAsync(ct);
		return true;
	}

	public async Task HandleCallbackQueryAsync(CallbackQuery callback, CancellationToken ct)
	{
		var update = new Update {Message = callback.Message, CallbackQuery = callback};
		var commandKey = "/" + callback.Data;

		if (_commands.TryGetValue(commandKey, out var handler))
			await handler(update, ct);
		else
			_telegramMessageFilter.Enqueue(callback.Message.Chat.Id, "Неизвестное действие 😅", ct: ct);

		await _botClient.AnswerCallbackQueryAsync(callback.Id, cancellationToken: ct);
	}

	public async Task SetBotCommandsAsync()
	{
		var botCommands = _commands.Keys
		                           .Where(c => c.StartsWith("/"))
		                           .Select(c => new BotCommand
		                           {
			                           Command = c.TrimStart('/'), Description = "Команда бота"
		                           })
		                           .ToArray();

		if (botCommands.Length != 0)
			await _botClient.SetMyCommandsAsync(botCommands);
	}

	private Func<Update, CancellationToken, Task> HandleStickerAsync(string stickerID)
		=> async (u, ct) => await _botClient.SendStickerAsync(u.Message.Chat.Id, stickerID, cancellationToken: ct);

	private async Task HandleStartAsync(Update update, CancellationToken ct)
	{
		var keyboard = new InlineKeyboardMarkup([
			new[]
			{
				InlineKeyboardButton.WithCallbackData("Привет 👋", "hello"),
				InlineKeyboardButton.WithCallbackData("Команды", "commands"),
				InlineKeyboardButton.WithCallbackData("🎯 Играть", "game"),
				InlineKeyboardButton.WithCallbackData("🧠 Викторина", "quiz")
			},
			new[]
			{
				InlineKeyboardButton.WithCallbackData("БАНДА 😎", "banda"),
				InlineKeyboardButton.WithCallbackData("Миша 😎", "misha"),
				InlineKeyboardButton.WithCallbackData("Ваня 😏", "vanya"),
				InlineKeyboardButton.WithCallbackData("Федя 😎", "fedya"),
				InlineKeyboardButton.WithCallbackData("Дима 😢", "dima"),
				InlineKeyboardButton.WithCallbackData("Грустно", "grystno")
			}
		]);
		_telegramMessageFilter.Enqueue(update.Message.Chat.Id, "Привет! Выбери действие:", replyMarkup: keyboard, ct);
	}

	private async Task HandleHelloAsync(Update update, CancellationToken ct)
	{
		string name = "";

		if (update.CallbackQuery != null)
		{
			// Если это CallbackQuery — берём пользователя из callback
			var user = update.CallbackQuery.From;
			name = $"{user.FirstName}{(string.IsNullOrEmpty(user.LastName) ? "" : $" {user.LastName}")}";
		}
		else if (update.Message != null)
		{
			var user = update.Message.From;
			if (user != null)
			{
				name = $"{user.FirstName}{(string.IsNullOrEmpty(user.LastName) ? "" : $" {user.LastName}")}";
			}
		}
		else
		{
			name = "друг";
		}

		_telegramMessageFilter.Enqueue(update.Message?.Chat.Id ?? update.CallbackQuery.Message.Chat.Id,
		                              $"Привет, {name}! 😄",
		                              ct: ct);
	}

	private async Task HandleGameAsync(Update update, CancellationToken ct)
	{
		var chatId = update.Message?.Chat.Id ?? update.CallbackQuery?.Message.Chat.Id;
		if (chatId == null)
			return;

		var emojis = new[] {"🎯", "🎲", "⚽️", "🏀", "🎳"};
		var random = new Random();
		var randomIndex = random.Next(emojis.Length);
		var emojiString = emojis[randomIndex];

		var emojiPack = new[] {Emoji.Darts, Emoji.Dice, Emoji.Football, Emoji.Basketball, Emoji.Bowling};
		var emoji = emojiPack[randomIndex];

		_telegramMessageFilter.Enqueue(chatId, $"Бросаем {emojiString}!", ct: ct);
		await _botClient.SendDiceAsync(chatId, emoji, cancellationToken: ct);
	}

	private async Task HandleQuizAsync(Update update, CancellationToken ct)
	{
		var chatId = update.Message?.Chat.Id ?? update.CallbackQuery?.Message.Chat.Id;
		if (chatId == null)
			return;

		// Пул вопросов
		var quizzes = new[]
		{
			new
			{
				Question = "Кто лучший программист? 💻",
				Options = new[] {"Миша", "Федя", "Дима", "Ваня"},
				Correct = 0,
				Explanation = "Очевидно 😎 — Миша лучший!"
			},
			new
			{
				Question = "Какой язык любит Дима? 🧠",
				Options = new[] {"C#", "Python", "Rust", "Assembler"},
				Correct = 1,
				Explanation = "Дима — питонист в душе 🐍"
			},
			new
			{
				Question = "Что выберет Федя? 🍕",
				Options = new[] {"Пиццу", "Работу", "Сон", "Танцы"},
				Correct = 0,
				Explanation = "Федя выбирает пиццу, как настоящий иммигрант 🇨🇦"
			}
		};

		var random = new Random();
		var quiz = quizzes[random.Next(quizzes.Length)];

		await _botClient.SendPollAsync(chatId,
		                               question: quiz.Question,
		                               options: quiz.Options,
		                               type: Telegram.Bot.Types.Enums.PollType.Quiz,
		                               correctOptionId: quiz.Correct,
		                               isAnonymous: false,
		                               explanation: quiz.Explanation,
		                               cancellationToken: ct);
	}

	private async Task HandleCommandsAsync(Update update, CancellationToken ct)
	{
		long userId;
		string userName;
		ChatType chatType;

		if (update.CallbackQuery != null)
		{
			if (update.CallbackQuery.From.IsBot)
			{
				_logger.LogDebug(update.CallbackQuery.From.IsBot.ToString());
				return;
			}

			userId = update.CallbackQuery.From.Id;
			userName = $"{update.CallbackQuery.From.FirstName}"
			           + $"{(string.IsNullOrEmpty(update.CallbackQuery.From.LastName) ? "" : $" {update.CallbackQuery.From.LastName}")}";

			// Если Callback пришёл из группы, всё равно отправляем личное сообщение пользователю
			chatType = ChatType.Private;
		}
		else if (update.Message != null)
		{
			if (update.Message.From.IsBot)
			{
				_logger.LogDebug(update.Message.From.FirstName);
				return;
			}

			userId = update.Message.From.Id;
			userName = $"{update.Message.From.FirstName}"
			           + $"{(string.IsNullOrEmpty(update.Message.From.LastName) ? "" : $" {update.Message.From.LastName}")}";
			chatType = update.Message.Chat.Type;
		}
		else
		{
			return;
		}

		// Формируем текст с командами
		var keywords = string.Join("\n", await _keywordHandler.GetAllKeywordsAsync());
		var tags = string.Join("\n", await _tagHandler.GetAllTags());

		var messageText = $"Привет, {userName}! 👋\n\n"
		                  + $"📜 Доступные команды:\n\n"
		                  + $"Обычные:\n{keywords}\n\n"
		                  + $"Тэг '(tag) @username @bot_name'\n{tags}";

		// Отправляем в зависимости от типа чата
		switch (chatType)
		{
			case ChatType.Private:
				_telegramMessageFilter.Enqueue(userId, messageText, ct: ct);
				break;

			case ChatType.Group:
			case ChatType.Supergroup:
				try
				{
					// В личку пользователю
					_telegramMessageFilter.Enqueue(userId, messageText, ct: ct);
				}
				catch (Telegram.Bot.Exceptions.ApiRequestException)
				{
					// Если не удалось отправить ЛС
					_telegramMessageFilter.Enqueue(update.Message.Chat.Id,
					                              $"{update.Message.From.FirstName}, не удалось отправить список команд в личку. "
					                              + $"Проверьте, включены ли у вас сообщения от ботов.",
					                              ct: ct);
				}

				break;
		}
	}
}