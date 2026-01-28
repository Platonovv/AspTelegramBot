using AspTelegramBot.Domain.Entities;
using AspTelegramBot.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace AspTelegramBot.Infrastructure.Repositories;

/// <summary>
/// Репозиторий для управления аудиофайлами Telegram (file_id).
/// </summary>
public class AudioRepository
{
	private readonly AppDbContext _db;
	private readonly IMemoryCache _cache;

	private const string CacheKey = "audioFiles";

	public AudioRepository(AppDbContext db, IMemoryCache cache)
	{
		_db = db;
		_cache = cache;
	}

#region GET

	public async Task<Dictionary<string, string>> GetAudioMapAsync()
	{
		return (await _cache.GetOrCreateAsync(CacheKey,
		                                      async entry =>
		                                      {
			                                      entry.SetSize(1);
			                                      entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5);
			                                      entry.Priority = CacheItemPriority.High;

			                                      var audioFiles = await _db.AudioFiles.ToListAsync();

			                                      return audioFiles.ToDictionary(
				                                      x => x.Key,
				                                      x => x.FileId,
				                                      StringComparer.OrdinalIgnoreCase);
		                                      }))!;
	}

	public async Task<AudioFile?> GetByKeyAsync(string key)
	{
		var map = await GetAudioMapAsync();
		return map.TryGetValue(key, out var fileId) ? new AudioFile {Key = key, FileId = fileId} : null;
	}

#endregion

#region POST

	public async Task AddAsync(AudioFile audio)
	{
		await _db.AudioFiles.AddAsync(audio);
		await _db.SaveChangesAsync();
		ClearCache();
	}

#endregion

#region DELETE

	public async Task DeleteByKeyAsync(string key)
	{
		var audio = await _db.AudioFiles.FirstOrDefaultAsync(x => x.Key == key);
		if (audio != null)
		{
			_db.AudioFiles.Remove(audio);
			await _db.SaveChangesAsync();
			ClearCache();
		}
	}

#endregion

	private void ClearCache()
	{
		_cache.Remove(CacheKey);
	}
}