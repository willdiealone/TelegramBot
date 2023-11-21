using System.Text;
using Application.Interfaces;
using Application.TGBotDtos.WeatherDtos;
using AutoMapper;
using Domain;
using MediatR;
using Persistence;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Application.UpdateTypeMessages.Messages;

public class TelegramBotMessageWeatherTomorrow
{
    public sealed class Query : IRequest<Unit>
    {
        // Сообшение пользователя
        public Message Message { get; set; }
        
        // id пользователя
        public long TelegramId { get; set; }
    }

    public sealed class Handler : IRequestHandler<Query, Unit>
    {
        // Маппер для перобразования дынных.
        private readonly IMapper _mapper;
        
        // Клиент для получения погодных данных
        private readonly IWeatherApiClientAccessor _weatherApiClientAccessor;
        
        // Прокидываем экземпляр бота
        private readonly ITelegramBotClient _bot;

        // Доступ к пользоваетелю из бд
        private readonly IUserAccessor _userAccessor;

        // Контекст базы данных
        private readonly DataContext _dataContext;
        
        // Доступ к кастомной клаиватуре
        private readonly IMyKeyboardMarkup _keyboardMarkup;

        // Доступ к коллекции эмадзиков ответа
        private readonly IEmojis _emojis;

        // Конструктор класса
        public Handler(IWeatherApiClientAccessor weatherApiClientAccessor,IMapper mapper,
            ITelegramBotClient bot,IUserAccessor userAccessor,
            DataContext dataContext, IEmojis emojis,
             IMyKeyboardMarkup keyboardMarkup)
        {
            _weatherApiClientAccessor = weatherApiClientAccessor;
            _mapper = mapper;
            _bot = bot;
            _userAccessor = userAccessor;
            _dataContext = dataContext;
            _emojis = emojis;
            _keyboardMarkup = keyboardMarkup;
        }
        
        // Метод для обработки запроса на получение погодных данных.
        public async Task<Unit> Handle(Query request, CancellationToken cancellationToken)
        {
            // Находим пользователя в бд по id
            var result = await _userAccessor.FindOrAddUserInDb(request.Message);
            // Проверяме план пользователя 
            if (result.Item1.Plan is null || result.Item1.Plan.PlansName != "Premium")
            {
                // Бот печатает
                await _bot.SendChatActionAsync(request.Message.From!.Id, ChatAction.Typing);
                // Ответ пользователю
                await _bot.SendTextMessageAsync(request.Message.From!.Id,
                    "\ud83d\udc77 <b>Для доступа требуется подписка, обратитесь к клавиатуре, в частности к кнопке подписка</b>",
                    0, ParseMode.Html,
                    replyMarkup: _keyboardMarkup.CreateReplyKeyboardMarkupWithoutPlan());
                return Unit.Value;
            }
            List[] list;
            WeatherDto weatherDto;
            
            // Проверяем состояние пользователя 
            switch (result.Item2)
            {
                // Если он ожидает локиции => 
                case var s when s == "WaitingForCityOrLocation" :
                    // Бот печатает
                    await _bot.SendChatActionAsync(request.Message.From!.Id, ChatAction.Typing);
                    // Ответ пользователю
                    await _bot.SendTextMessageAsync(request.Message.From!.Id, 
                        "\u270d\ufe0f <b>Напишите название населенного пункта или отправьте свою геолокацию,"
                        + " чтобы я показал погоду</b>",0,ParseMode.Html,
                        replyMarkup: _keyboardMarkup.CreateOnlyLocationKeyboardMarkup());
                    break;
                
                // Если он укомплектован =>
                case var s when s == "ComleatedUser" :
                    
                    // Отправяляем запрос для получения данных о погоде
                    list = await _weatherApiClientAccessor.GetWeatherForTomowrrowAsync(result.Item1.Location,
                        cancellationToken);
                            
                    // Проверяем список на наличие эелементов
                    if (list.Length > 0)
                    {
                        // Сзсдаем кнопку поделиться ботом
                        var keyboard = _keyboardMarkup.InlineShareMessageOrBot();
                        // Создаем билдер для формирования ответа
                        StringBuilder builder = new StringBuilder();
                        // Проходим по списку маппим данные и в зависимости от этих данных формируем ответ
                        for (int i = 0; i < list.Length; i++)
                        {
                            weatherDto = _mapper.Map<List, WeatherDto>(list[i]);
                            var emoji = _emojis.GetEmoji(weatherDto.Description);
                            if (weatherDto.Time == "09:00")
                            {
                                builder.Append(
                                    $"<b>\ud83d\udcc5 {weatherDto.Date}({weatherDto.DayOfWeek})\n\n" +
                                    $"\ud83d\udc81\u200d\u2642\ufe0f Погода утром: {weatherDto.Temperature}\u00b0" +
                                    $"\n{emoji}</b>" +
                                    $"\n\n<code>Ощущается как: {weatherDto.FeelsLike}" +
                                    $"\nСкорость ветра: {weatherDto.Speed} м/с" +
                                    $"\nВлажность: {weatherDto.Humidity}%\n" +
                                    $"Давление: {weatherDto.Pressure} мм рт.ст.</code>\n\n");
                                continue;
                            }

                            if (weatherDto.Time == "15:00")
                            {
                                builder.Append(
                                    $"<b>Погода днем: {weatherDto.Temperature}\u00b0" +
                                    $"\n{emoji}</b>" +
                                    $"\n\n<code>Ощущается как: {weatherDto.FeelsLike}" +
                                    $"\nСкорость ветра: {weatherDto.Speed} м/с" +
                                    $"\nВлажность: {weatherDto.Humidity}%\n" +
                                    $"Давление: {weatherDto.Pressure} мм рт.ст.</code>\n\n");
                                continue;
                            }

                            if (weatherDto.Time == "21:00")
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

                        // Бот печатает
                        await _bot.SendChatActionAsync(request.Message.From!.Id, ChatAction.Typing);
                        // Возвращаем ответ пользователю
                        await _bot.SendTextMessageAsync(request.TelegramId, builder.ToString(),
                            0, ParseMode.Html, replyMarkup: keyboard);
                    }
                    else // В случае если данных нет => 
                    {
                        // Делаем поьзователя ожидающим локиацию
                        result.Item1.State = UserState.WaitingForCityOrLocation;
                        // Соохраняем в бд
                        var seccess =  await _dataContext.SaveChangesAsync() > 0;
                        // Если успешно
                        if (seccess)
                        {
                            // Бот печатает
                            await _bot.SendChatActionAsync(request.Message.From!.Id, ChatAction.Typing);
                            // Возвращаем ответ пользователю
                            await _bot.SendTextMessageAsync(request.Message.From!.Id, 
                                "\ud83d\udc77 <b>Извините, но мы не смогли дозониться " +
                                "или мы не можем получить погоду в это регионе, попроуйте изменить адрес или отправьте геолокацию!</b>",
                                0,ParseMode.Html,replyMarkup: _keyboardMarkup.CreateOnlyLocationKeyboardMarkup());
                        }
                        else
                        {
                            // Бот печатает
                            await _bot.SendChatActionAsync(request.Message.From!.Id, ChatAction.Typing);
                            // Возвращаем ответ пользователю
                            await _bot.SendTextMessageAsync(request.Message.From!.Id, 
                                "\ud83d\udc77 <b>Извините, но мы не смогли дозониться " +
                                "или мы не можем получить погоду в это регионе, попроуйте изменить адрес или отправьте геолокацию!</b>",
                                0,ParseMode.Html,replyMarkup: _keyboardMarkup.CreateOnlyLocationKeyboardMarkup());
                        }
                    }
                    break;
            }
            return Unit.Value;
        }
    }
}