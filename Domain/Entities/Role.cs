using System.ComponentModel.DataAnnotations;

namespace AspTelegramBot.Domain.Entities;

/// <summary>
/// Представляет сущность «Роль» в системе.
/// </summary>
public class Role
{
	[Key]
	public Guid Id { get; set; }

	[Required]
	[StringLength(50)]
	public string Name { get; set; } = string.Empty;

	public ICollection<User> Users { get; set; } = new List<User>();
}