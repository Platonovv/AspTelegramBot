using AspTelegramBot.Application.DTOs.Users;
using AspTelegramBot.Application.Interfaces;
using AspTelegramBot.Application.Interfaces.ForUser;
using AspTelegramBot.Domain.Entities;
using AspTelegramBot.Infrastructure.Repositories;
using Microsoft.AspNetCore.Identity;
using Telegram.Bot;
using Telegram.Bot.Types;
using User = AspTelegramBot.Domain.Entities.User;

/// <summary>
/// Обрабатывает команды, предназначенные для администраторов.
/// </summary>
public class AdminHandler
{
	private readonly IUserService _userService;
	private readonly IRoleService _roleService;
	private readonly TelegramBotClient _botClient;
	private readonly BotPhrasesRepository _repository;
	private readonly IUserRoleService _userRoleService;
	private readonly IPasswordHasher<User> _passwordHasher;

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

	public async Task<bool> HandleAdminCommand(Update update, string messageText, CancellationToken ct)
	{
		if (update.Message == null)
			return false;

		var userId = update.Message.From.Id;

		// ===== ADD / REMOVE PHRASES =====
		if (messageText.StartsWith("/addphrase "))
		{
			if (await CheckFromRoles(ct, userId, ["Admin", "Moderator"]))
				return true;

			var args = messageText["/addphrase ".Length..].Split(";", 2); // формат: триггер;ответ
			if (args.Length < 2)
			{
				await _botClient.SendTextMessageAsync(userId,
				                                      "Используй формат: /addphrase триггер;ответ",
				                                      cancellationToken: ct);
				return true;
			}

			var trigger = args[0].Trim();
			var response = args[1].Trim();

			await _repository.AddPhraseAsync(new BotPhrase
			{
				TriggerText = trigger,
				ResponseText = response,
				Category = "keyword"
			});

			await _botClient.SendTextMessageAsync(userId, $"Фраза '{trigger}' добавлена!", cancellationToken: ct);
			return true;
		}

		if (messageText.StartsWith("/removephrase "))
		{
			if (await CheckFromRoles(ct, userId, ["Admin", "Moderator"]))
				return true;

			var trigger = messageText["/removephrase ".Length..].Trim();

			var removed = await _repository.RemovePhraseAsync(trigger, "keyword");
			if (removed)
				await _botClient.SendTextMessageAsync(userId, $"Фраза '{trigger}' удалена!", cancellationToken: ct);
			else
				await _botClient.SendTextMessageAsync(userId, $"Фраза '{trigger}' не найдена.", cancellationToken: ct);
			return true;
		}

		// ===== CREATE USER =====
		if (messageText.StartsWith("/createuser "))
		{
			if (await CheckFromRoles(ct, userId, ["Admin"]))
				return true;

			// Формат: /createuser Имя;Email;Возраст
			var args = messageText["/createuser ".Length..].Split(";", 3);
			if (args.Length < 3 || !int.TryParse(args[2].Trim(), out int age))
			{
				await _botClient.SendTextMessageAsync(userId,
				                                      "Используй формат: /createuser Имя;Email;Возраст",
				                                      cancellationToken: ct);
				return true;
			}

			var user = new User
			{
				Name = args[0].Trim(), Email = args[1].Trim(), Age = age, TelegramID = update.Message.From.Id
			};

			var password = $"{1234f}";
			user.PasswordHash = _passwordHasher.HashPassword(user, password);
			var createdUser = await _userService.AddUserAsync(user);
			await _botClient.SendTextMessageAsync(userId,
			                                      $"Пользователь {createdUser.Name} создан с ID {createdUser.Id}",
			                                      cancellationToken: ct);
			return true;
		}

		// ===== ADD ROLE TO USER =====
		if (messageText.StartsWith("/addrole "))
		{
			if (await CheckFromRoles(ct, userId, ["Admin"]))
				return true;

			// Формат: /addrole UserId;RoleName
			var args = messageText["/addrole ".Length..].Split(";", 2);
			if (args.Length < 2 || !Guid.TryParse(args[0].Trim(), out var targetUserId))
			{
				await _botClient.SendTextMessageAsync(userId,
				                                      "Используй формат: /addrole UserId;RoleName",
				                                      cancellationToken: ct);
				return true;
			}

			var roleName = args[1].Trim();
			var role = await _roleService.GetRoleByNameAsync(roleName);
			if (role == null)
			{
				await _botClient.SendTextMessageAsync(userId, $"Роль '{roleName}' не найдена.", cancellationToken: ct);
				return true;
			}

			var user = await _userService.GetUserByIdAsync(targetUserId);
			await _userRoleService.AssignRolesToUserAsync(user!, [role]);
			await _botClient.SendTextMessageAsync(userId,
			                                      $"Пользователю {targetUserId} присвоена роль {role.Name}.",
			                                      cancellationToken: ct);
			return true;
		}

		// ===== REMOVE ROLE FROM USER =====
		if (messageText.StartsWith("/removerole "))
		{
			if (await CheckFromRoles(ct, userId, ["Admin"]))
				return true;

			// Формат: /removerole UserId;RoleName
			var args = messageText["/removerole ".Length..].Split(";", 2);
			if (args.Length < 2 || !Guid.TryParse(args[0].Trim(), out var targetUserId))
			{
				await _botClient.SendTextMessageAsync(userId,
				                                      "Используй формат: /removerole UserId;RoleName",
				                                      cancellationToken: ct);
				return true;
			}

			var roleName = args[1].Trim();
			var role = await _roleService.GetRoleByNameAsync(roleName);
			if (role == null)
			{
				await _botClient.SendTextMessageAsync(userId, $"Роль '{roleName}' не найдена.", cancellationToken: ct);
				return true;
			}

			var user = await _userService.GetUserByIdAsync(targetUserId);
			await _userRoleService.RemoveRolesFromUserAsync(user!);
			await _botClient.SendTextMessageAsync(userId,
			                                      $"Роль {role.Name} удалена у пользователя {targetUserId}.",
			                                      cancellationToken: ct);
			return true;
		}

		return false;
	}

	private async Task<bool> CheckFromRoles(CancellationToken ct, long telegramID, List<string> roles)
	{
		var user = await _userService.GetUserByIdAsync(telegramID);

		if (user == null)
		{
			await _botClient.SendTextMessageAsync(telegramID, "Ты не зарегистрирован!", cancellationToken: ct);
			return true;
		}

		IEnumerable<UserResponseDto> admins = null;
		IEnumerable<UserResponseDto> moderators = null;

		foreach (var role in roles)
		{
			switch (role)
			{
				case "Admin":
					admins = await _userRoleService.GetUsersByRoleAsync("Admin");
					break;
				case "Moderator":
					moderators = await _userRoleService.GetUsersByRoleAsync("Moderator");
					break;
			}
		}

		var isAdmin = admins != null && admins.Any(x => x.Id == user.Id);
		var isModerator = (moderators != null && moderators.Any(x => x.Id == user.Id));
		var isInRoles = isAdmin || isModerator;

		if (isInRoles)
			return false;

		var response = roles.Contains("Admin") ? "У тебя нет прав администратора!" : "У тебя нет прав модератора!";

		await _botClient.SendTextMessageAsync(telegramID, response, cancellationToken: ct);
		return true;
	}
}