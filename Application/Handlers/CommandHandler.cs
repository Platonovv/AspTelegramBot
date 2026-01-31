using AspTelegramBot.Application.Filters;
using AspTelegramBot.Application.Interfaces.ForHandler;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace AspTelegramBot.Application.Handlers;

public class CommandHandler : IUpdateHandler
{
	private readonly TelegramBotClient _botClient;
	private readonly TagHandler _tagHandler;
	private readonly KeywordHandler _keywordHandler;
	private readonly TelegramMessageFilter _telegramMessageFilter;

	private readonly Dictionary<string, Func<Update, CancellationToken, Task>> _commands;

	public CommandHandler(TelegramBotClient botClient,
	                      KeywordHandler keywordHandler,
	                      TagHandler tagHandler,
	                      TelegramMessageFilter telegramMessageFilter)
	{
		_botClient = botClient;
		_tagHandler = tagHandler;
		_keywordHandler = keywordHandler;
		_telegramMessageFilter = telegramMessageFilter;

		_commands = new Dictionary<string, Func<Update, CancellationToken, Task>>(StringComparer.OrdinalIgnoreCase)
		{
			{"start", HandleStartAsync},
			{"commands", HandleCommandsAsync},
			{"hello", HandleHelloAsync},
			{"game", HandleGameAsync},
			{"quiz", HandleQuizAsync},
			{"misha", HandleStickerAsync("CAACAgIAAxkBAAM3aQyJ-u2Z13Z73C39Yvw5dpRaM-oAAuyGAAKs9ElIiV1v7bo2cLY2BA")},
			{"vanya", HandleStickerAsync("CAACAgIAAxkBAAM5aQyKAvVEobWnRk3ta9cf4NcHjzkAAll_AAJXF0BISg4y0sxlb9I2BA")},
			{"fedya", HandleStickerAsync("CAACAgIAAxkBAAM7aQyKC0GtkI-cKC9SKJ0ktPfx_tIAAtKFAAJg-kBI4Jr_ecEGYvk2BA")},
			{
				"grystno",
				HandleStickerAsync("CAACAgIAAxkBAAM_aQyKQ44sLT1MAAFaUgZMqOYBCIBiAAKePwACKxrBSQ9tMdUadxVKNgQ")
			},
			{"dima", HandleStickerAsync("CAACAgIAAxkBAAIB5mkNxtB8NI4IlrFzT4SJ6WGurMujAAJ7jwACgVFoSBFwCUl_EhndNgQ")},
			{"banda", HandleStickerAsync("CAACAgIAAxkBAAIB6GkNx5piapWOhSHoRuu5psPWPl6zAAJweAACTKNxSLvmKsNWvEm_NgQ")}
		};
	}

	public async Task SetBotCommandsAsync()
	{
		var botCommands = new List<BotCommand>
		{
			new() {Command = "start", Description = "Запустить бота"},
			new() {Command = "commands", Description = "Список доступных команд"},
			new() {Command = "hello", Description = "Приветствие"},
			new() {Command = "game", Description = "Игровая мини-игра"},
			new() {Command = "quiz", Description = "Викторина"},
			new() {Command = "misha", Description = "Стикер Миша"},
			new() {Command = "vanya", Description = "Стикер Ваня"},
			new() {Command = "fedya", Description = "Стикер Федя"},
			new() {Command = "grystno", Description = "Стикер Грустно"},
			new() {Command = "dima", Description = "Стикер Дима"},
			new() {Command = "banda", Description = "Стикер Банда"}
		};

		await _botClient.SetMyCommandsAsync(botCommands);
	}

	public async Task<bool> HandleAsync(Update update, CancellationToken ct)
	{
		string? rawText = update.Message?.Text ?? update.CallbackQuery?.Data;
		var chatId = update.Message?.Chat.Id ?? update.CallbackQuery?.Message?.Chat.Id;

		if (rawText == null || chatId == null)
			return false;

		// Убираем / и @BotUsername
		var key = rawText.TrimStart('/');
		if (key.Contains("@"))
			key = key.Split('@')[0];

		if (_commands.TryGetValue(key, out var handler))
		{
			await handler(update, ct);
			return true;
		}

		return false;
	}

	private Func<Update, CancellationToken, Task> HandleStickerAsync(string stickerId)
		=> async (update, ct) =>
		{
			var chatId = update.Message?.Chat.Id ?? update.CallbackQuery?.Message.Chat.Id;
			if (chatId != null)
				await _botClient.SendStickerAsync(chatId, stickerId, cancellationToken: ct);
		};

	private Task HandleStartAsync(Update update, CancellationToken ct)
	{
		var chatId = update.Message?.Chat.Id ?? update.CallbackQuery?.Message.Chat.Id;
		if (chatId == null)
			return Task.CompletedTask;

		var keyboard = new InlineKeyboardMarkup(new[]
		{
			new[]
			{
				InlineKeyboardButton.WithCallbackData(
					"Привет 👋",
					"hello"),
				InlineKeyboardButton.WithCallbackData(
					"Команды",
					"commands"),
				InlineKeyboardButton.WithCallbackData(
					"🎯 Играть",
					"game"),
				InlineKeyboardButton.WithCallbackData(
					"🧠 Викторина",
					"quiz")
			},
			new[]
			{
				InlineKeyboardButton.WithCallbackData(
					"БАНДА 😎",
					"banda"),
				InlineKeyboardButton.WithCallbackData(
					"Миша 😎",
					"misha"),
				InlineKeyboardButton.WithCallbackData(
					"Ваня 😏",
					"vanya"),
				InlineKeyboardButton.WithCallbackData(
					"Федя 😎",
					"fedya"),
				InlineKeyboardButton
					.WithCallbackData("Дима 😢", "dima"),
				InlineKeyboardButton.WithCallbackData(
					"Грустно",
					"grystno")
			}
		});

		_telegramMessageFilter.Enqueue(chatId, "Привет! Выбери действие:", keyboard, ct);
		return Task.CompletedTask;
	}

	private Task HandleHelloAsync(Update update, CancellationToken ct)
	{
		var chatId = update.Message?.Chat.Id ?? update.CallbackQuery?.Message.Chat.Id;
		if (chatId == null)
			return Task.CompletedTask;

		var user = update.Message?.From ?? update.CallbackQuery?.From;
		var name = user != null ? $"{user.FirstName} {user.LastName}".Trim() : "друг";

		_telegramMessageFilter.Enqueue(chatId, $"Привет, {name}! 😄", ct: ct);
		return Task.CompletedTask;
	}

	private Task HandleGameAsync(Update update, CancellationToken ct)
	{
		var chatId = update.Message?.Chat.Id ?? update.CallbackQuery?.Message.Chat.Id;
		if (chatId == null)
			return Task.CompletedTask;

		var diceTypes = new[] {Emoji.Darts, Emoji.Dice, Emoji.Football, Emoji.Basketball, Emoji.Bowling};
		var emojis = new[] {"🎯", "🎲", "⚽️", "🏀", "🎳"};
		var idx = new Random().Next(diceTypes.Length);

		_telegramMessageFilter.Enqueue(chatId, $"Бросаем {emojis[idx]}!", ct: ct);
		return _botClient.SendDiceAsync(chatId, diceTypes[idx], cancellationToken: ct);
	}

	private async Task HandleQuizAsync(Update update, CancellationToken ct)
	{
		var chatId = update.Message?.Chat.Id ?? update.CallbackQuery?.Message.Chat.Id;
		if (chatId == null)
			return;

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

		var quiz = quizzes[new Random().Next(quizzes.Length)];
		await _botClient.SendPollAsync(chatId,
		                               question: quiz.Question,
		                               options: quiz.Options,
		                               type: PollType.Quiz,
		                               correctOptionId: quiz.Correct,
		                               isAnonymous: false,
		                               explanation: quiz.Explanation,
		                               cancellationToken: ct);
	}

	private async Task HandleCommandsAsync(Update update, CancellationToken ct)
	{
		var chatId = update.Message?.Chat.Id ?? update.CallbackQuery?.Message.Chat.Id;
		if (chatId == null)
			return;

		var userName = update.Message?.From != null
			               ? $"{update.Message.From.FirstName} {update.Message.From.LastName}".Trim()
			               : $"{update.CallbackQuery.From.FirstName} {update.CallbackQuery.From.LastName}".Trim();

		var keywords = string.Join("\n", await _keywordHandler.GetAllKeywordsAsync());
		var tags = string.Join("\n", await _tagHandler.GetAllTags());

		var message
			= $"Привет, {userName}! 👋\n\n📜 Доступные команды:\n\nОбычные:\n{keywords}\n\nТэг '(tag) @username @bot_name'\n{tags}";
		_telegramMessageFilter.Enqueue(chatId, message, ct: ct);
	}
}