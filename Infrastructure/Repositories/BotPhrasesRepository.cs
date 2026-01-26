using System.Text.RegularExpressions;
using AspTelegramBot.Domain.Entities;
using AspTelegramBot.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace AspTelegramBot.Infrastructure.Repositories;

/// <summary>
/// Класс предназначен для управления фразами бота, их добавления, удаления и получения из базы данных.
/// </summary>
public class BotPhrasesRepository
{
	private readonly AppDbContext _db;
	private readonly IMemoryCache _cache;

	public BotPhrasesRepository(AppDbContext db, IMemoryCache cache)
	{
		_db = db;
		_cache = cache;
	}

#region Get

	public async Task<List<(Regex regex, string key, string response)>> GetKeywordRegexAsync()
	{
		return (await _cache.GetOrCreateAsync("compiledKeywords",
		                                      async entry =>
		                                      {
			                                      entry.SetSize(1);
			                                      entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(1);
			                                      entry.Priority = CacheItemPriority.High;

			                                      var keywords = await _db.BotPhrases
			                                                              .Where(p => p.Category == "keyword")
			                                                              .OrderByDescending(p => p.TriggerText.Length)
			                                                              .ToListAsync();

			                                      return keywords
			                                             .Select(k => (regex: new Regex($@"\b{Regex.Escape(k.TriggerText)}(?!\w)", RegexOptions.IgnoreCase | RegexOptions.Compiled),
				                                                          key: k.TriggerText, response: k.ResponseText))
			                                             .ToList();
		                                      }))!;
	}

	public async Task<Dictionary<string, string>> GetGroupKeywordsAsync()
	{
		return (await _cache.GetOrCreateAsync("groupKeywords",
		                                      async entry =>
		                                      {
			                                      entry.SetSize(1);
			                                      entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(1);
			                                      entry.Priority = CacheItemPriority.High;

			                                      var phrases = await _db.BotPhrases
			                                                             .Where(p => p.Category == "group")
			                                                             .ToListAsync();

			                                      return phrases.ToDictionary(
				                                      p => p.TriggerText,
				                                      p => p.ResponseText,
				                                      StringComparer.OrdinalIgnoreCase);
		                                      }))!;
	}

	public async Task<Dictionary<string, List<string>>> GetTagsAsync()
	{
		return (await _cache.GetOrCreateAsync("tags",
		                                      async entry =>
		                                      {
			                                      entry.SetSize(1);
			                                      entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(1);
			                                      entry.Priority = CacheItemPriority.High;

			                                      var tags = await _db.BotPhrases
			                                                          .Where(p => p.Category == "tag")
			                                                          .ToListAsync();

			                                      return tags
			                                             .GroupBy(x => x.TriggerText, StringComparer.OrdinalIgnoreCase)
			                                             .ToDictionary(g => g.Key,
			                                                           g => g.First().ResponseText.Split("|").ToList(),
			                                                           StringComparer.OrdinalIgnoreCase);
		                                      }))!;
	}

#endregion

#region POST

	public async Task AddPhraseAsync(BotPhrase botPhrase)
	{
		await _db.BotPhrases.AddAsync(botPhrase);
		await _db.SaveChangesAsync();

		ClearCache();
	}

#endregion

#region DELETE

	public async Task<bool> RemovePhraseAsync(string trigger, string category)
	{
		var phrase = await _db.BotPhrases.FirstOrDefaultAsync(p => p.TriggerText == trigger && p.Category == category);
		if (phrase == null)
			return false;

		_db.BotPhrases.Remove(phrase);
		await _db.SaveChangesAsync();

		ClearCache();
		return true;
	}

#endregion

	private void ClearCache()
	{
		_cache.Remove("keywords");
		_cache.Remove("groupKeywords");
		_cache.Remove("tags");
		_cache.Remove("compiledKeywords");
	}
}