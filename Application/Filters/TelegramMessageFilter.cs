using System.Collections.Concurrent;
using System.Threading.Channels;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types.ReplyMarkups;

namespace AspTelegramBot.Application.Filters
{
	/// <summary>
	/// Фильтрует и обрабатывает отправку сообщений через Telegram-бота с учетом ограничения частоты и дедупликации сообщений.
	/// </summary>
	public class TelegramMessageFilter
	{
		private readonly TelegramBotClient _botClient;
		private readonly Channel<MessageItem> _queue;
		private readonly int _defaultDelayMs;

		// Rate-limiting по пользователю (любое сообщение)
		private readonly ConcurrentDictionary<long?, DateTime> _lastSentPerUser = new();
		private readonly int _userCooldownSeconds;

		// Дедупликация одинаковых сообщений в рамках чата
		private readonly ConcurrentDictionary<(long?, string), bool> _recentMessages = new();
		private readonly int _messageDedupSeconds;

		private record MessageItem(long? ChatId, string Text, InlineKeyboardMarkup? ReplyMarkup, CancellationToken Ct);

		public TelegramMessageFilter(TelegramBotClient botClient,
		                             int defaultDelayMs = 1000,
		                             int maxQueueSize = 200,
		                             int userCooldownSeconds = 1,
		                             int messageDedupSeconds = 2)
		{
			_botClient = botClient;
			_defaultDelayMs = defaultDelayMs;
			_userCooldownSeconds = userCooldownSeconds;
			_messageDedupSeconds = messageDedupSeconds;

			_queue = Channel.CreateBounded<MessageItem>(new BoundedChannelOptions(maxQueueSize)
			{
				FullMode = BoundedChannelFullMode.DropOldest
			});

			_ = Task.Run(ProcessQueueAsync);
		}

		public void Enqueue(long? chatId,
		                    string text,
		                    InlineKeyboardMarkup? replyMarkup = null,
		                    CancellationToken ct = default)
		{
			if (string.IsNullOrWhiteSpace(text))
				return;

			// Rate-limiting по пользователю (любое сообщение)
			if (_lastSentPerUser.TryGetValue(chatId, out var lastUserMsg)
			    && (DateTime.UtcNow - lastUserMsg).TotalSeconds < _userCooldownSeconds)
				return;

			_lastSentPerUser[chatId] = DateTime.UtcNow;

			// Дедупликация одинаковых сообщений в рамках одного чата
			var key = (chatId, text);
			if (!_recentMessages.TryAdd(key, true))
				return;

			// Очистка дедупликации через messageDedupSeconds
			_ = Task.Run(async () =>
			             {
				             try
				             {
					             await Task.Delay(_messageDedupSeconds * 1000, ct);
					             _recentMessages.TryRemove(key, out _);
				             }
				             catch (TaskCanceledException)
				             {
				             }
			             },
			             ct);

			_queue.Writer.TryWrite(new MessageItem(chatId, text, replyMarkup, ct));
		}

		private async Task ProcessQueueAsync()
		{
			await foreach (var item in _queue.Reader.ReadAllAsync())
			{
				try
				{
					await _botClient.SendTextMessageAsync(
						                item.ChatId,
						                item.Text,
						                replyMarkup: item.ReplyMarkup,
						                cancellationToken: item.Ct)
					                .WaitAsync(item.Ct);
				}
				catch (ApiRequestException ex) when (ex.ErrorCode == 429)
				{
					// Too Many Requests
					var retrySeconds = ex.Parameters?.RetryAfter ?? (_defaultDelayMs / 1000);
					await Task.Delay(retrySeconds * 1000, item.Ct);

					try
					{
						await _botClient.SendTextMessageAsync(
							                item.ChatId,
							                item.Text,
							                replyMarkup: item.ReplyMarkup,
							                cancellationToken: item.Ct)
						                .WaitAsync(item.Ct);
					}
					catch
					{
					}
				}
				catch
				{
				}

				// Минимальная задержка между сообщениями
				await Task.Delay(_defaultDelayMs, item.Ct);
			}
		}
	}
}