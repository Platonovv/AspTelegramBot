using AspTelegramBot.Application.Interfaces;
using AspTelegramBot.Application.Interfaces.ForHandler;
using AspTelegramBot.Application.Interfaces.ForUser;
using AspTelegramBot.Domain.Entities;
using AspTelegramBot.Infrastructure.Repositories;
using Microsoft.AspNetCore.Identity;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using User = AspTelegramBot.Domain.Entities.User;

namespace AspTelegramBot.Application.Handlers;

/// <summary>
/// Обрабатывает команды, предназначенные для администраторов.
/// </summary>
public class AdminHandler : IUpdateHandler
{
	private readonly IUserService _userService;
	private readonly IRoleService _roleService;
	private readonly IUserRoleService _userRoleService;
	private readonly TelegramBotClient _botClient;
	private readonly BotPhrasesRepository _repository;
	private readonly IPasswordHasher<User> _passwordHasher;

	public ChatAction ChatAction => ChatAction.Typing;

	public AdminHandler(TelegramBotClient botClient,
	                    BotPhrasesRepository repository,
	                    IUserService userService,
	                    IUserRoleService userRoleService,
	                    IRoleService roleService,
	                    IPasswordHasher<User> passwordHasher)
	{
		_botClient = botClient;
		_repository = repository;
		_userService = userService;
		_roleService = roleService;
		_passwordHasher = passwordHasher;
		_userRoleService = userRoleService;
	}

	public async Task<bool> HandleAsync(Update update, CancellationToken ct)
	{
		if (update.Message == null || string.IsNullOrEmpty(update.Message.Text))
			return false;

		var userId = update.Message.From.Id;
		var messageText = update.Message.Text.Trim();

		await _botClient.SendChatActionAsync(update.Message.Chat.Id, ChatAction, cancellationToken: ct);

		// ===== Add / Remove phrase =====
		if (messageText.StartsWith("/addphrase "))
			return await HandleAddPhrase(update, messageText, ct, userId);

		if (messageText.StartsWith("/removephrase "))
			return await HandleRemovePhrase(update, messageText, ct, userId);

		// ===== User management =====
		if (messageText.StartsWith("/createuser "))
			return await HandleCreateUser(update, messageText, ct, userId);

		if (messageText.StartsWith("/addrole "))
			return await HandleAddRole(update, messageText, ct, userId);

		if (messageText.StartsWith("/removerole "))
			return await HandleRemoveRole(update, messageText, ct, userId);

		return false;
	}

	private async Task<bool> HandleAddPhrase(Update update, string messageText, CancellationToken ct, long userId)
	{
		if (await CheckFromRoles(ct, userId, new List<string> {"Admin", "Moderator"}))
			return true;

		var args = messageText["/addphrase ".Length..].Split(";", 3);
		if (args.Length < 3)
		{
			await _botClient.SendTextMessageAsync(userId,
			                                      "Используй формат: /addphrase триггер;ответ;категория (keyword/group/tag)",
			                                      cancellationToken: ct);
			return true;
		}

		await _repository.AddPhraseAsync(new BotPhrase
		{
			TriggerText = args[0].Trim(),
			ResponseText = args[1].Trim(),
			Category = args[2].Trim()
		});

		await _botClient.SendTextMessageAsync(userId, $"Фраза '{args[0].Trim()}' добавлена!", cancellationToken: ct);
		return true;
	}

	private async Task<bool> HandleRemovePhrase(Update update, string messageText, CancellationToken ct, long userId)
	{
		if (await CheckFromRoles(ct, userId, new List<string> {"Admin", "Moderator"}))
			return true;

		var args = messageText["/removephrase ".Length..].Split(";", 2);
		var removed = await _repository.RemovePhraseAsync(args[0].Trim(), args[1].Trim());

		await _botClient.SendTextMessageAsync(userId,
		                                      removed
			                                      ? $"Фраза '{args[0].Trim()}' удалена!"
			                                      : $"Фраза '{args[0].Trim()}' не найдена.",
		                                      cancellationToken: ct);

		return true;
	}

	private async Task<bool> HandleCreateUser(Update update, string messageText, CancellationToken ct, long userId)
	{
		if (await CheckFromRoles(ct, userId, new List<string> {"Admin"}))
			return true;

		var args = messageText["/createuser ".Length..].Split(";", 3);
		if (args.Length < 3 || !int.TryParse(args[2], out int age))
		{
			await _botClient.SendTextMessageAsync(userId,
			                                      "Используй формат: /createuser Имя;Email;Возраст",
			                                      cancellationToken: ct);
			return true;
		}

		var user = new User
		{
			Name = args[0].Trim(),
			Email = args[1].Trim(),
			Age = age,
			TelegramID = update.Message.From.Id,
			PasswordHash = _passwordHasher.HashPassword(null!, "1234")
		};

		var createdUser = await _userService.AddUserAsync(user);
		await _botClient.SendTextMessageAsync(userId,
		                                      $"Пользователь {createdUser.Name} создан с ID {createdUser.Id}",
		                                      cancellationToken: ct);

		return true;
	}

	private async Task<bool> HandleAddRole(Update update, string messageText, CancellationToken ct, long userId)
	{
		if (await CheckFromRoles(ct, userId, new List<string> {"Admin"}))
			return true;

		var args = messageText["/addrole ".Length..].Split(";", 2);
		if (args.Length < 2 || !Guid.TryParse(args[0], out var targetUserId))
		{
			await _botClient.SendTextMessageAsync(userId,
			                                      "Используй формат: /addrole UserId;RoleName",
			                                      cancellationToken: ct);
			return true;
		}

		var role = await _roleService.GetRoleByNameAsync(args[1].Trim());
		if (role == null)
		{
			await _botClient.SendTextMessageAsync(userId,
			                                      $"Роль '{args[1].Trim()}' не найдена.",
			                                      cancellationToken: ct);
			return true;
		}

		var user = await _userService.GetUserByIdAsync(targetUserId);
		await _userRoleService.AssignRolesToUserAsync(user!, new List<Role> {role});

		await _botClient.SendTextMessageAsync(userId,
		                                      $"Пользователю {targetUserId} присвоена роль {role.Name}.",
		                                      cancellationToken: ct);
		return true;
	}

	private async Task<bool> HandleRemoveRole(Update update, string messageText, CancellationToken ct, long userId)
	{
		if (await CheckFromRoles(ct, userId, new List<string> {"Admin"}))
			return true;

		var args = messageText["/removerole ".Length..].Split(";", 2);
		if (args.Length < 2 || !Guid.TryParse(args[0], out var targetUserId))
		{
			await _botClient.SendTextMessageAsync(userId,
			                                      "Используй формат: /removerole UserId;RoleName",
			                                      cancellationToken: ct);
			return true;
		}

		var role = await _roleService.GetRoleByNameAsync(args[1].Trim());
		if (role == null)
		{
			await _botClient.SendTextMessageAsync(userId,
			                                      $"Роль '{args[1].Trim()}' не найдена.",
			                                      cancellationToken: ct);
			return true;
		}

		var user = await _userService.GetUserByIdAsync(targetUserId);
		await _userRoleService.RemoveRolesFromUserAsync(user!);

		await _botClient.SendTextMessageAsync(userId,
		                                      $"Роль {role.Name} удалена у пользователя {targetUserId}.",
		                                      cancellationToken: ct);
		return true;
	}

	private async Task<bool> CheckFromRoles(CancellationToken ct, long telegramID, List<string> roles)
	{
		var user = await _userService.GetUserByIdAsync(telegramID);
		if (user == null)
		{
			await _botClient.SendTextMessageAsync(telegramID, "Ты не зарегистрирован!", cancellationToken: ct);
			return true;
		}

		var hasRole = false;
		foreach (var role in roles)
		{
			var usersWithRole = await _userRoleService.GetUsersByRoleAsync(role);
			if (usersWithRole.Any(u => u.Id == user.Id))
				hasRole = true;
		}

		if (!hasRole)
		{
			var response = roles.Contains("Admin") ? "У тебя нет прав администратора!" : "У тебя нет прав модератора!";
			await _botClient.SendTextMessageAsync(telegramID, response, cancellationToken: ct);
			return true;
		}

		return false;
	}
}