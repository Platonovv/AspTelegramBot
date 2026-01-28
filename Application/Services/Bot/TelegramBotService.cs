using AspTelegramBot.Application.Filters;
using AspTelegramBot.Application.Handlers;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using IUpdateHandler = AspTelegramBot.Application.Interfaces.ForHandler.IUpdateHandler;

namespace AspTelegramBot.Application.Services.Bot;

/// <summary>
/// Класс для работы с Telegram Bot API по новому паттерну IUpdateHandler.
/// </summary>
public class TelegramBotService
{
	private readonly TelegramBotClient _botClient;
	private readonly IServiceScopeFactory _scopeFactory;
	private readonly TelegramMessageFilter _telegramMessageFilter;
	private string? _botUsername;

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
		                          new ReceiverOptions
		                          {
			                          AllowedUpdates = Array.Empty<UpdateType>(), ThrowPendingUpdates = true
		                          });

		Console.WriteLine($"Бот @{_botUsername} запущен!");
	}

	private Task HandleErrorAsync(ITelegramBotClient client, Exception ex, CancellationToken ct)
	{
		Console.WriteLine($"Ошибка: {ex.Message}");
		return Task.CompletedTask;
	}

	private async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken ct)
	{
		using var scope = _scopeFactory.CreateScope();

		// Все хендлеры
		var allHandlers = new List<IUpdateHandler>
		{
			scope.ServiceProvider.GetRequiredService<AdminHandler>(),
			scope.ServiceProvider.GetRequiredService<CommandHandler>(),
			scope.ServiceProvider.GetRequiredService<KeywordHandler>(),
			scope.ServiceProvider.GetRequiredService<TagHandler>(),
			scope.ServiceProvider.GetRequiredService<AudioHandler>(),
			scope.ServiceProvider.GetRequiredService<GroupImportantBotHandler>(),
			scope.ServiceProvider.GetRequiredService<StickerHandler>()
		};

		var groupHandlers = allHandlers.Where(x => x is GroupImportantBotHandler or AudioHandler or CommandHandler)
		                               .ToList();

		var messageText = update.Message?.Text?.Trim() ?? update.CallbackQuery?.Data;
		var chatId = update.Message?.Chat.Id ?? update.CallbackQuery?.Message?.Chat.Id;
		var chatType = update.Message?.Chat.Type;
		var isGroupChat = chatType is ChatType.Group or ChatType.Supergroup;

		// Стикеры обрабатываем только в личном чате
		if (!isGroupChat && update.Message?.Type == MessageType.Sticker)
		{
			var stickerHandler = allHandlers.OfType<StickerHandler>().FirstOrDefault();
			if (stickerHandler != null && await stickerHandler.HandleAsync(update, ct))
				return;
		}

		if (messageText == null || chatId == null)
			return;

		var isMentioned = messageText.Contains($"@{_botUsername}", StringComparison.OrdinalIgnoreCase);

		// ===== Группы =====

		if (isGroupChat)
		{
			foreach (var groupHandler in groupHandlers)
			{
				if (await groupHandler.HandleAsync(update, ct))
					return;
			}
		}
		// ===== Личные сообщения =====
		else
		{
			foreach (var handler in allHandlers)
			{
				if (handler.GetType() == typeof(GroupImportantBotHandler))
					continue;

				if (await handler.HandleAsync(update, ct))
					return;
			}
		}

		// Ответ по умолчанию
		if (chatType == ChatType.Private || isMentioned)
			_telegramMessageFilter.Enqueue(chatId, "Не знаю такой команды 😅.", ct: ct);
	}
}