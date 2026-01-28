using AspTelegramBot.Application.Filters;
using AspTelegramBot.Application.Handlers;
using AspTelegramBot.Application.Interfaces;
using AspTelegramBot.Application.Interfaces.ForUser;
using AspTelegramBot.Application.Services;
using AspTelegramBot.Application.Services.Bot;
using AspTelegramBot.Application.Services.ForUser;
using AspTelegramBot.Domain.Entities;
using AspTelegramBot.Domain.Interfaces;
using AspTelegramBot.Infrastructure.Database;
using AspTelegramBot.Infrastructure.Repositories;
using FluentValidation.AspNetCore;
using Hangfire;
using Hangfire.MemoryStorage;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Telegram.Bot;

public class Program
{
	public static async Task Main(string[] args)
	{
		var builder = WebApplication.CreateBuilder(args);

		// -----------------------
		// Serilog Logging
		// -----------------------
		Log.Logger = new LoggerConfiguration().MinimumLevel
		                                      .Debug()
		                                      .WriteTo
		                                      .Console()
		                                      .WriteTo
		                                      .File("logs/log-.txt", rollingInterval: RollingInterval.Day)
		                                      .Enrich
		                                      .FromLogContext()
		                                      .CreateLogger();

		builder.Host.UseSerilog();

		// -----------------------
		// AutoMapper
		// -----------------------
		builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

		// -----------------------
		// Configuration & DB
		// -----------------------
		DotNetEnv.Env.Load();
		var connectionString = Environment.GetEnvironmentVariable("DB_CONNECTION")
		                       ?? $"Host={Environment.GetEnvironmentVariable("DB_HOST")};Port={Environment.GetEnvironmentVariable("DB_PORT")};Database={Environment.GetEnvironmentVariable("POSTGRES_DB")};Username={Environment.GetEnvironmentVariable("POSTGRES_USER")};Password={Environment.GetEnvironmentVariable("POSTGRES_PASSWORD")}";
		builder.Services.AddDbContext<AppDbContext>(options => options.UseNpgsql(connectionString));

		// -----------------------
		// MemoryCache (для UserService)
		// -----------------------
		builder.Services.AddMemoryCache();

		// -----------------------
		// Repositories / Services DI
		// -----------------------
		builder.Services.AddScoped<IUserRoleService, UserRoleService>();
		builder.Services.AddScoped<IUserRepository, EfUserRepository>();
		builder.Services.AddScoped<IUserService, UserService>();
		builder.Services.AddScoped<IRoleRepository, EfRoleRepository>();
		builder.Services.AddScoped<IRoleService, RoleService>();
		builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();

		// -----------------------
		// Validators
		// -----------------------
		builder.Services.AddFluentValidationAutoValidation();
		builder.Services.AddFluentValidationClientsideAdapters();

		// -----------------------
		// Hangfire
		// -----------------------
		builder.Services.AddHangfire(config => config.UseMemoryStorage());
		builder.Services.AddHangfireServer();

		// -----------------------
		// Health Checks
		// -----------------------
		builder.Services.AddHealthChecks();

		// -----------------------
		// Swagger
		// -----------------------
		builder.Services.AddControllers();
		builder.Services.AddEndpointsApiExplorer();
		builder.Services.AddSwaggerGen();

		// -----------------------
		// Telegram Bot Service
		// -----------------------
		var botClient = new TelegramBotClient(Environment.GetEnvironmentVariable("TELEGRAM_TOKEN")!); // fallback на env

		builder.Services.AddSingleton(botClient);

		//Repository
		builder.Services.AddScoped<BotPhrasesRepository>();
		builder.Services.AddScoped<AudioRepository>();

		//Handlers
		builder.Services.AddScoped<AudioHandler>();
		builder.Services.AddScoped<KeywordHandler>();
		builder.Services.AddScoped<TagHandler>();
		builder.Services.AddScoped<GroupImportantBotHandler>();
		builder.Services.AddScoped<CommandHandler>();
		builder.Services.AddScoped<AdminHandler>();
		builder.Services.AddScoped<StickerHandler>();

		builder.Services.AddSingleton<TelegramMessageFilter>();
		builder.Services.AddSingleton<TelegramBotService>();

		builder.Services.AddMemoryCache();
		// -----------------------
		// Add Authorization Service
		// -----------------------
		builder.Services.AddAuthorization();

		var app = builder.Build();

		// -----------------------
		// Apply Migrations
		// -----------------------
		using (var scope = app.Services.CreateScope())
		{
			var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
			// Просто проверяем, что база доступна
			await dbContext.Database.CanConnectAsync();
		}

		// -----------------------
		// Middleware & Swagger
		// -----------------------
		app.UseSwagger();
		app.UseSwaggerUI();
		app.UseHttpsRedirection();
		app.UseAuthorization();

		// -----------------------
		// Health Check Endpoint
		// -----------------------
		app.MapHealthChecks("/health",
		                    new HealthCheckOptions
		                    {
			                    ResponseWriter = async (context, report) =>
			                    {
				                    context.Response.ContentType = "application/json";
				                    await context.Response.WriteAsJsonAsync(new
				                    {
					                    status = report.Status.ToString(),
					                    checks
						                    = report.Entries.Select(e => new
						                    {
							                    e.Key,
							                    e.Value.Status,
							                    e.Value.Description
						                    })
				                    });
			                    }
		                    });

		// -----------------------
		// Telegram Bot Start (await обязательно!)
		// -----------------------
		var botService = app.Services.GetRequiredService<TelegramBotService>();
		await botService.StartAsync();

		// -----------------------
		// Controllers
		// -----------------------
		app.MapControllers();

		// -----------------------
		// Run App
		// -----------------------
		await app.RunAsync();
	}
}