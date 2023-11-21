using System.Text;
using Application.Interfaces;
using Application.TGBotDtos.WeatherDtos;
using AutoMapper;
using Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Application.UpdateTypeMessages.Messages;

public class TelegramBotMessageWeatherNow
{
	public sealed class Query : IRequest<Unit>
	{
		// Сообшение пользователя
		public static Message Message { get; set; }
		
		// id пользователя
		public static long TelegramId { get; set; }
	}
	
	public sealed class Handler : IRequestHandler<Query, Unit>
	{
		// Маппер для перобразования дынных
		private readonly IMapper _mapper;

		// Сревис для получения погодных данных
		private readonly IWeatherApiClientAccessor _weatherApiClientAccessor;

		// Инстанс телерам бота
		private readonly ITelegramBotClient _bot;

		// Доступ к пользоваетелю из бд
		private readonly IUserAccessor _userAccessor;

		// Контекст базы данных
		private readonly DataContext _dataContext;

		// Доступк к дефолтной клавиатуре
		private readonly IMyKeyboardMarkup _myKeyboardMarkup;

		// Доступ к коллекции эмоздиков
		private readonly IEmojis _emojis;

		// Конструктор класса
		public Handler(IWeatherApiClientAccessor weatherApiClientAccessor, IMapper mapper,
				ITelegramBotClient bot, IUserAccessor userAccessor,
				DataContext dataContext, IEmojis emojis, IMyKeyboardMarkup myKeyboardMarkup)
		{
			_weatherApiClientAccessor = weatherApiClientAccessor;
			_mapper = mapper;
			_bot = bot;
			_userAccessor = userAccessor;
			_dataContext = dataContext;
			_emojis = emojis;
			_myKeyboardMarkup = myKeyboardMarkup;
		}

		/// <summary>
		/// Метод для обработки запроса на получение погодных данных.
		/// </summary>
		/// <param name="request">Свойсвто из запроса</param>
		/// <param name="cancellationToken">Токен отмены</param>
		/// <returns>Перечисление данных о погоде для нескольких городов</returns>
		public async Task<Unit> Handle(Query request, CancellationToken cancellationToken)
		{
			// Находим пользователя в бд по id
			var result = await _userAccessor.FindOrAddUserInDb(Query.Message);
			List[] list;
			WeatherDto weatherDto;
			// Проверяем состояние пользователя 
			switch (result.Item2)
			{
				// Если он ожидает локиции => 
				case var s when s == "WaitingForCityOrLocation":
					// Бот печатает
					await _bot.SendChatActionAsync(Query.Message.From!.Id, ChatAction.Typing);
					// Ответ пользователю
					await _bot.SendTextMessageAsync(Query.Message.From!.Id,
							"\u270d\ufe0f <b>Напишите название населенного пункта или отправьте свою геолокацию,"
							+ " чтобы я показал погоду</b>", 0, ParseMode.Html,
							replyMarkup: _myKeyboardMarkup.CreateOnlyLocationKeyboardMarkup());
					break;

				// Если он укомплектован =>
				case var s when s == "ComleatedUser":
					// Отправялем запрос для получения погоды
					list = await _weatherApiClientAccessor.GetWeatherNowAsync(result.Item1.Location, cancellationToken);
					// Строка для подстановки в ответ
					string now = "\ud83d\udc81\u200d\u2642\ufe0f Погода сейчас";
					// Провермяем наличие данных в ответе
					if (list.Length > 0)
					{
						// Сосздаем кнопку поделиться ботом
						var keyboard = _myKeyboardMarkup.InlineShareMessageOrBot();
						// СОздаем билдер для формирования ответа
						StringBuilder builder = new StringBuilder();
						// Проходим по списку маппим данные и в зависимости от этих данных формируем ответ
						for (int i = 0; i < list.Length; i++)
						{
							weatherDto = _mapper.Map<List, WeatherDto>(list[i]);
							var emoji = _emojis.GetEmoji(weatherDto.Description);
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
						// Бот печатает
						await _bot.SendChatActionAsync(Query.Message.From!.Id, ChatAction.Typing);
						// Ответ пользователю
						await _bot.SendTextMessageAsync(Query.TelegramId, builder.ToString(),
						0, ParseMode.Html, replyMarkup: keyboard);
						
					}
					else // В случае если данных нет => 
					{
						// Делаем поьзователя ожидающим локиацию
						result.Item1.State = UserState.WaitingForCityOrLocation;
						// Соохраняем в бд
						var seccess = await _dataContext.SaveChangesAsync() > 0;
						// Если успешно
						if (seccess)
						{
							// Бот печатает
							await _bot.SendChatActionAsync(Query.Message.From!.Id, ChatAction.Typing);
							// Ответ пользователю
							await _bot.SendTextMessageAsync(Query.Message.From!.Id,
								"\ud83d\udc77 <b>Извините, но мы не смогли дозониться " +
								"или мы не можем получить погоду в это регионе, попробуйте изменить адрес или отправьте геолокацию!</b>",
								0, ParseMode.Html, replyMarkup: _myKeyboardMarkup.CreateOnlyLocationKeyboardMarkup());
						}
					}
					break;
			}
			
			return Unit.Value;	
		}
	}
}