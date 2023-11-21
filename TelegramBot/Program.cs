global using static System.Console;
using Application.UpdateTypeCallbackQuerys;
using Application.UpdateTypeCommands;
using Application.UpdateTypeLocation;
using Application.UpdateTypeMessages;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Microsoft.Extensions.DependencyInjection;
using Persistence;
using Serilog;
using TelegramBot.Extentions;

// Создаем токен отмены
using CancellationTokenSource cts = new();

// Настройки получения всех типы обновлений, кроме обновлений, связанных с ChatMember
ReceiverOptions receiverOptions = new()
{
	AllowedUpdates = Array.Empty<UpdateType>()
};

// Регистрируем создаем "appsettings.json" в конфигурации
IConfiguration myConfig = new ConfigurationBuilder()
	.SetBasePath("/Users/lilrockstar/RiderProjects/TelegramBot/TelegramBot/")
	.AddJsonFile("appsettings.json")
	.AddJsonFile("launchSettings.json").Build();

// Создаем сервисы
var serviceCollections = new ServiceCollection();

// Создаем сервисы
serviceCollections.AddTelegramBotClientExtentions(myConfig);

// Провайдер сервисов
var service = serviceCollections.BuildServiceProvider();

// Получаем инстанс бота
var botClient = service.GetRequiredService<ITelegramBotClient>();

// Получаем медиатор
var mediator = service.GetRequiredService<IMediator>();

var mifrationProgress = myConfig["profiles:Migrate:environmentVariables:MIGRATION_IN_PROGRESS"];

Log.Logger = new LoggerConfiguration()
	.WriteTo.Async(wt => wt.Console())
	.CreateLogger();

if (mifrationProgress != "true")
{
	botClient.StartReceiving(
		// Начинаем прослушивать (HandleUpdatesAsync: метод который будет обрабатывать обновлене бота,
		updateHandler: HandleUpdatesAsync,
		// HandleErrorAsync: метод который будет обрабатывать ошибки,
		pollingErrorHandler: HandlePollingErrorAsync,
		// receiverOptions: передаем конфигурацию настроек обновлений,
		receiverOptions: receiverOptions,
		//cts.Token, токен отмены для адекватного прерывания обработки сообщений)
		cancellationToken: cts.Token
	);
	
	// Получаем экземпляр бота
	var me = await botClient.GetMeAsync();

	// Лог
	Log.Information("Начал прослушку @{MeUsername}", me.Username);
	ReadLine();

	// Ожидаем отмену потока 
	cts.Cancel();

	// Метод обработки сообщений =>
	async Task HandleUpdatesAsync(ITelegramBotClient bot, Update update, CancellationToken cancellation)
	{
		var serviceProvider = service;
		switch (update.Type)
		{
			case UpdateType.Message:
				if (update.Message.Text is not null && update.Message.Text.StartsWith("/"))
					await CommandsHandler.HandleCommands(botClient, update.Message, mediator);
				else if (update.Message.Text is not null && !update.Message.Text.StartsWith("/"))
				 await MessagesHandler.HandleMessage(update.Message, mediator);
				else if (update.Message.Location is not null && update.Message.Location is Location location)
				{
					await mediator.Send(new HandlerMessageLocation.Query
					{
						TelegramUserId = update.Message.From!.Id,
						Message = update.Message,
						Location = $"{location.Latitude},{location.Longitude}"
					});
				}
				break;
			case UpdateType.CallbackQuery:
				if (update.CallbackQuery is not null)
				await CallbackQueryHandler.HandleCallbackQuery(botClient, mediator,update.CallbackQuery,serviceProvider);
				break;
		}
	}

	// Метод обработки ошибок
	Task HandlePollingErrorAsync(ITelegramBotClient bot, Exception exception, CancellationToken cancellationTokenSource)
	{
		// Проверяем ошибку
		var errorMessage = exception switch
		{
			// Если ошибка апи телеграма, возвращаем сообщение с ошибкой
			// если какая либо другая ошибка, вовзвращаем станд. сообщение об ошибке
			ApiRequestException apiRequestException => $"Ошибка телеграмм: \n{apiRequestException.Message}",
			_ => exception.ToString()
		};
		WriteLine(errorMessage);
		return Task.CompletedTask;
	}
}
else
{
	var provider = serviceCollections.BuildServiceProvider();
	try
	{
		var context = provider.GetRequiredService<DataContext>();
		await context.Database.MigrateAsync();
	}
	catch (Exception e)
	{
		WriteLine(e.Message);
	}
}