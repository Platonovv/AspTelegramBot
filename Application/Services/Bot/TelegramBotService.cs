using AspTelegramBot.Application.Filters;
using AspTelegramBot.Application.Handlers;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace AspTelegramBot.Application.Services.Bot;

/// <summary>
/// Класс, предоставляющий основной функционал работы с Telegram Bot API.
/// </summary>
public class TelegramBotService
{
	private readonly TelegramBotClient _botClient;

	private string? _botUsername;
	private readonly IServiceScopeFactory _scopeFactory;
	private readonly TelegramMessageFilter _telegramMessageFilter;

	public TelegramBotService(TelegramBotClient botClient,
	                          IServiceScopeFactory scopeFactory,
	                          TelegramMessageFilter telegramMessageFilter)
	{
		_botClient = botClient;
		_scopeFactory = scopeFactory;
		_telegramMessageFilter = telegramMessageFilter;
	}

	public async Task StartAsync()
	{
		var me = await _botClient.GetMeAsync();
		_botUsername = me.Username;

		using var scope = _scopeFactory.CreateScope();
		var commandHandler = scope.ServiceProvider.GetRequiredService<CommandHandler>();
		await commandHandler.SetBotCommandsAsync();

		_botClient.StartReceiving(HandleUpdateAsync,
		                          HandleErrorAsync,
		                          new ReceiverOptions {AllowedUpdates = [], ThrowPendingUpdates = true});

		Console.WriteLine($"Бот @{_botUsername} запущен!");
	}

	private Task HandleErrorAsync(ITelegramBotClient client, Exception ex, CancellationToken ct)
	{
		Console.WriteLine($"Ошибка: {ex.Message}");
		return Task.CompletedTask;
	}

	private async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken ct)
	{
		if (update.Message != null)
		{
			using var scope = _scopeFactory.CreateScope();

			// Берём Scoped сервисы внутри scope
			var adminHandler = scope.ServiceProvider.GetRequiredService<AdminHandler>();
			var commandHandler = scope.ServiceProvider.GetRequiredService<CommandHandler>();
			var keywordHandler = scope.ServiceProvider.GetRequiredService<KeywordHandler>();
			var tagHandler = scope.ServiceProvider.GetRequiredService<TagHandler>();
			var groupHandler = scope.ServiceProvider.GetRequiredService<GroupImportantBotHandler>();

			var messageText = update.Message.Text?.Trim() ?? "";
			var chatType = update.Message.Chat.Type;
			var isMentioned = messageText.Contains($"@{_botUsername}", StringComparison.OrdinalIgnoreCase);

			// В группе реагируем только на упоминания или важные сообщения
			if (chatType is ChatType.Group or ChatType.Supergroup && !isMentioned && !messageText.StartsWith("/"))
			{
				await groupHandler.HandleKeyword(update, messageText, ct);
				return;
			}

			switch (update.Message.Type)
			{
				case MessageType.Sticker:
					_telegramMessageFilter.Enqueue(update.Message.Chat.Id,
					                               $"FileId стикера:\n{update.Message.Sticker.FileId}",
					                               ct: ct);

					return;
			}

			// Убираем @username
			var cleanedText = messageText.Replace($"@{_botUsername}", "", StringComparison.OrdinalIgnoreCase).Trim();
			cleanedText = string.Join(' ', cleanedText.Split(' ', StringSplitOptions.RemoveEmptyEntries));

			// Проверка Админки
			if (await adminHandler.HandleAdminCommand(update, cleanedText, ct))
				return;

			// Проверка команд
			if (await commandHandler.HandleCommand(update, cleanedText, ct))
				return;

			// Проверка ключевых слов из базы
			if (await keywordHandler.HandleKeyword(update, cleanedText, ct))
				return;

			// Проверка тегов с делегатами
			if (await tagHandler.HandleTagAsync(update, cleanedText, ct))
				return;

			// Если личка или упомянут — ответ по умолчанию
			if (chatType == ChatType.Private || isMentioned)
				_telegramMessageFilter.Enqueue(update.Message.Chat.Id, "Не знаю такой команды 😅.", ct: ct);
		}
		else if (update.CallbackQuery != null)
		{
			using var scope = _scopeFactory.CreateScope();
			var commandHandler = scope.ServiceProvider.GetRequiredService<CommandHandler>();
			await commandHandler.HandleCallbackQueryAsync(update.CallbackQuery, ct);
		}
	}
}