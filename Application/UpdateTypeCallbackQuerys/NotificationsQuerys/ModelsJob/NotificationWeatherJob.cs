using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Application.Interfaces;
using Application.TGBotDtos.WeatherDtos;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Persistence;
using Quartz;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;

namespace Application.UpdateTypeCallbackQuerys.NotificationsQuerys.ModelsJob;

public class NotificationWeatherJob : IJob
{
	public static long TelegramId { get; set; }
	public static IServiceProvider ServiceProvider { get; set; }
	
	public async Task Execute(IJobExecutionContext context)
	{
		 
		var dataContext = ServiceProvider.GetRequiredService<DataContext>();
		var weatherApiClientAccessor = ServiceProvider.GetRequiredService<IWeatherApiClientAccessor>();
		var inlineKeyboardMarkup = ServiceProvider.GetRequiredService<IMyKeyboardMarkup>();
		var mapper = ServiceProvider.GetRequiredService<IMapper>();
		var emojis = ServiceProvider.GetRequiredService<IEmojis>();
		var bot = ServiceProvider.GetRequiredService<ITelegramBotClient>();
		
		CancellationToken cancellationToken = new CancellationToken();
		
		List[] list; 
		WeatherDto weatherDto; 
		var users = await dataContext.Users.FirstOrDefaultAsync(u => u.UserId == TelegramId); 
		list = await weatherApiClientAccessor.GetWeatherNowAsync(users.Location, cancellationToken); 
		string now = "\ud83d\udc81\u200d\u2642\ufe0f Погода сейчас";
		if (list.Length > 0)
		{
			var keyboard = inlineKeyboardMarkup.InlineShareMessageOrBot();
			StringBuilder builder = new StringBuilder();
			for (int i = 0; i < list.Length; i++)
			{
				// Перобразование полученных погодных данных в DTO обьект
				weatherDto = mapper.Map<List, WeatherDto>(list[i]);
				var emoji = emojis.GetEmoji(weatherDto.Description);
				if (weatherDto.Time == "09:00")
				{
					builder.Append(
						$"<b>{now}: {weatherDto.Temperature}\u00b0" +
						$"\n{emoji}</b>" +
						$"\n\n<code>Ощущается как: {weatherDto.FeelsLike}" +
						$"\nСкорость ветра: {weatherDto.Speed} м/с" +
						$"\nВлажность: {weatherDto.Humidity}%\n" +
						$"Давление: {weatherDto.Pressure} мм рт.ст.</code>\n\n");
					continue;
				}

				if (weatherDto.Time == "15:00")
				{
					if (list.Length == 2)
					{
						builder.Append(
							$"<b>{now}: {weatherDto.Temperature}\u00b0" +
							$"\n{emoji}</b>" +
							$"\n\n<code>Ощущается как: {weatherDto.FeelsLike}" +
							$"\nСкорость ветра: {weatherDto.Speed} м/с" +
							$"\nВлажность: {weatherDto.Humidity}%\n" +
							$"Давление: {weatherDto.Pressure} мм рт.ст.</code>\n\n");
					}
					else
					{
						builder.Append(
							$"<b>Погода днем: {weatherDto.Temperature}\u00b0" +
							$"\n{emoji}</b>" +
							$"\n\n<code>Ощущается как: {weatherDto.FeelsLike}" +
							$"\nСкорость ветра: {weatherDto.Speed} м/с" +
							$"\nВлажность: {weatherDto.Humidity}%\n" +
							$"Давление: {weatherDto.Pressure} мм рт.ст.</code>\n\n");
					}

					continue;
				}

				if (weatherDto.Time == "21:00")
				{
					if (list.Length == 1)
					{
						builder.Append(
							$"<b>{now}: {weatherDto.Temperature}\u00b0" +
							$"\n{emoji}</b>" +
							$"\n\n<code>Ощущается как: {weatherDto.FeelsLike}" +
							$"\nСкорость ветра: {weatherDto.Speed} м/с" +
							$"\nВлажность: {weatherDto.Humidity}%\n" +
							$"Давление: {weatherDto.Pressure} мм рт.ст.</code>\n");
					}
					else
					{
						builder.Append(
							$"<b>Погода ночью: {weatherDto.Temperature}\u00b0" +
							$"\n{emoji}</b>" +
							$"\n\n<code>Ощущается как: {weatherDto.FeelsLike}" +
							$"\nСкорость ветра: {weatherDto.Speed} м/с" +
							$"\nВлажность: {weatherDto.Humidity}%\n" +
							$"Давление: {weatherDto.Pressure} мм рт.ст.</code>\n");
					}
				}
			}
			await bot.SendChatActionAsync(TelegramId, ChatAction.Typing);
			await bot.SendTextMessageAsync(TelegramId, builder.ToString(),
				0, ParseMode.Html, replyMarkup: keyboard);
		}
	}
}