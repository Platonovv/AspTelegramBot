using AspTelegramBot.Domain.Entities;
using AspTelegramBot.Infrastructure.Repositories;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.InputFiles;
using File = System.IO.File;
using System.Security.Cryptography;
using AspTelegramBot.Application.Interfaces.ForHandler;
using Telegram.Bot.Types.Enums;

namespace AspTelegramBot.Application.Handlers;

/// <summary>
/// Обрабатывает аудио-команды по ключевым словам.
/// </summary>
public class AudioHandler : IUpdateHandler
{
	private readonly TelegramBotClient _botClient;
	private readonly AudioRepository _repository;

	public ChatAction ChatAction => ChatAction.RecordVoice;

	public AudioHandler(TelegramBotClient botClient, AudioRepository repository)
	{
		_botClient = botClient;
		_repository = repository;
	}

	public async Task<bool> HandleAsync(Update update, CancellationToken ct)
	{
		if (update.Message == null || string.IsNullOrWhiteSpace(update.Message.Text))
			return false;

		var chatId = update.Message.Chat.Id;
		var key = update.Message.Text.ToLower().Trim();
		var filePath = Path.Combine("Audio", $"{key}.mp3");

		if (!File.Exists(filePath))
			return false;

		// Вычисляем хэш файла
		string fileHash;
		await using (var stream = File.OpenRead(filePath))
		using (var md5 = MD5.Create())
		{
			var hashBytes = await md5.ComputeHashAsync(stream, ct);
			fileHash = BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
		}

		await _botClient.SendChatActionAsync(chatId, ChatAction, cancellationToken: ct);

		// Проверяем базу
		var audioFromDb = await _repository.GetByKeyAsync(key);
		if (audioFromDb != null && audioFromDb.FileHash == fileHash)
		{
			// Файл не менялся — используем старый FileId
			await _botClient.SendAudioAsync(chatId, audioFromDb.FileId, cancellationToken: ct);
			return true;
		}

		if (audioFromDb != null)
		{
			// Файл изменился — удаляем старую запись
			await _repository.DeleteByKeyAsync(audioFromDb.Key);
		}

		// Отправляем новый файл
		await using var newStream = File.OpenRead(filePath);
		var message = await _botClient.SendAudioAsync(chatId,
		                                              new InputOnlineFile(newStream, $"{key}.mp3"),
		                                              cancellationToken: ct);

		// Сохраняем новый FileId и hash
		if (message.Audio != null)
		{
			var file = new AudioFile
			{
				Key = key, FileId = message.Audio.FileId, FileHash = fileHash, CreatedAt = DateTime.UtcNow
			};
			await _repository.AddAsync(file);
		}

		return true;
	}
}