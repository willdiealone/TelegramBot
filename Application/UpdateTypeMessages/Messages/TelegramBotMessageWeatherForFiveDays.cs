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

public class TelegramBotMessageWeatherForFiveDays
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
        
        // Сервис получения погоднях данных
        private readonly IWeatherApiClientAccessor _weatherApiClientAccessor;
        
        // Инстанс телеграм бота
        private readonly ITelegramBotClient _bot;

        // Доступ к пользователю из бд по id
        private readonly IUserAccessor _userAccessor;

        // Контекст базы даных
        private readonly DataContext _dataContext;

        // Доступ к кастомной клавиатуре
        private readonly IMyKeyboardMarkup _myKeyboardMarkup;

        // Доступ к эмодзикам ответа
        private readonly IEmojis _emojis;

        // Конструктор класса
        public Handler(IWeatherApiClientAccessor weatherApiClientAccessor,IMapper mapper,
            ITelegramBotClient bot,IUserAccessor userAccessor, DataContext dataContext, IEmojis emojis,
            IMyKeyboardMarkup myKeyboardMarkup)
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
                    replyMarkup: _myKeyboardMarkup.CreateReplyKeyboardMarkupWithoutPlan());
                return Unit.Value;
            }
            
            // Проверяем состояние пользователя
            switch (result.Item2)
            {
                // Если пользователь ожиает локацию => 
                case var s when s == "WaitingForCityOrLocation" :
                    // Бот печатает
                    await _bot.SendChatActionAsync(request.Message.From!.Id, ChatAction.Typing);
                    // Возвращаем ответ
                    await _bot.SendTextMessageAsync(request.Message.From!.Id, 
                        "\u270d\ufe0f <b>Напишите название населенного пункта или отправьте свою геолокацию,"
                        + " чтобы я показал погоду</b>",0,ParseMode.Html,
                        replyMarkup: _myKeyboardMarkup.CreateOnlyLocationKeyboardMarkup());
                    break;
                
                // Если не ожидает локацию => 
                case var s when s == "ComleatedUser" :
                    
                    // Отправляем запрос для получение погодных данных
                    var arrayList = await _weatherApiClientAccessor.GetWeatherForFiveDaysAsync(result.Item1.Location, cancellationToken);
                    // Проверяем массив с даными
                    if (arrayList.Length > 2)
                    {
                        // Сосздаем кнопку поделиться ботом
                        var keyboard = _myKeyboardMarkup.InlineShareMessageOrBot();
                        // Создаем билдер для формирования ответа 
                        StringBuilder builder = new StringBuilder();
                        // Инициализируем массив 10-ю элементами 
                        var weatherDto = new WeatherDto[10];
                        // Проходим по массиву ответа, маппим данные и в зависимости от данных формируем ответ
                        for (int i = 0; i < arrayList.Length; i++)
                        {
                            weatherDto[i] = _mapper.Map<List,WeatherDto>(arrayList[i]);
                            var emoji = _emojis.GetEmoji(weatherDto[i].Description);
                            if (i % 2 == 0)
                            {
                                builder.Append($"<b>\ud83d\udcc5 {weatherDto[i].Date} ({weatherDto[i].DayOfWeek})</b>" +
                                               $"\nДнём {weatherDto[i].Temperature}\u00b0 {emoji}");
                            }
                            else
                            {
                                builder.Append($"\nНочью {weatherDto[i].Temperature}\u00b0 {emoji}\n\n");
                            }
                        }
                        // Бот печатает
                        await _bot.SendChatActionAsync(request.Message.From!.Id, ChatAction.Typing);
                        // Возвращаем ответ
                        await _bot.SendTextMessageAsync(request.TelegramId,builder.ToString(),
                            0,ParseMode.Html, replyMarkup: keyboard);
                        
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
                                0,ParseMode.Html,replyMarkup: _myKeyboardMarkup.CreateOnlyLocationKeyboardMarkup());
                        }   
                    }
                    break;
            }
            return Unit.Value;
        }
    }
}