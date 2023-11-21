using System.Text.RegularExpressions;
using Application.Interfaces;
using Application.UpdateTypeCallbackQuerys.NotificationsQuerys.ModelsJob;
using MediatR;
using Persistence;
using Quartz;
using Serilog;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Application.UpdateTypeMessages.Messages;

public class TelegramBotMessageNotifications
{
    public sealed class Query : IRequest<Unit>
    {
        // Сообщение пользователя
        public Message Message { get; set; }
        
        // id пользователя
        public long TelegramId { get; set; }
    }

    public sealed class Handler : IRequestHandler<Query, Unit>
    {
        // Доступк пользователю из бд по id 
        private readonly IUserAccessor _userAccessor;
        
        // Доступк к инстансу телеграм бота
        private readonly ITelegramBotClient _bot;

        // Доступ к кастомной клавиатуре
        private readonly IMyKeyboardMarkup _myKeyboardMarkup;
        
        // Доступ к геолокации пользователя
        private readonly ILocationAccessor _locationAccessor;

        private readonly IScheduler _scheduler;

        // Конструктор
        public Handler(IUserAccessor userAccessor, ITelegramBotClient bot, DataContext dataContext,
            ILocationAccessor locationAccessor, IMyKeyboardMarkup myKeyboardMarkup, IScheduler scheduler)
        {
            _userAccessor = userAccessor;
            _bot = bot;
            _locationAccessor = locationAccessor;
            _myKeyboardMarkup = myKeyboardMarkup;
            _scheduler = scheduler;
        }
        
        public async Task<Unit> Handle(Query request, CancellationToken cancellationToken)
        {
            // Находим пользователя в бд по id
            var result = await _userAccessor.FindOrAddUserInDb(request.Message);
            // Проверяем план пользователя
            if (result.Item1.Plan is null || result.Item1.Plan.PlansName != "Premium")
            {
                // Бот печатает
                await _bot.SendChatActionAsync(request.Message.From!.Id, ChatAction.Typing,cancellationToken:cancellationToken);
                // Ответ пользователю
                await _bot.SendTextMessageAsync(request.Message.From!.Id,
                    "\ud83d\udc77 <b>Для доступа требуется подписка, обратитесь к клавиатуре, в частности к кнопке подписка</b>",
                    0, ParseMode.Html,
                    replyMarkup: _myKeyboardMarkup.CreateReplyKeyboardMarkupWithoutPlan(),cancellationToken:cancellationToken);
                return Unit.Value;
            }
            
            // Инициализируем переменную для хранения локации пользователя
            string location = String.Empty;
            // Проверяем состояние пользователя
            switch (result.Item2)
            {
                // Если он ожидает локиции => 
                case var s when s == "WaitingForCityOrLocation":
                    
                    // Бот печатает
                    await _bot.SendChatActionAsync(request.Message.From!.Id, ChatAction.Typing,cancellationToken:cancellationToken);
                    // Ответ пользователю
                    await _bot.SendTextMessageAsync(request.Message.From!.Id, 
                        "\u270d\ufe0f <b>Напишите название населенного пункта или отправьте свою геолокацию,"
                        + " чтобы я показал погоду</b>",0,ParseMode.Html,
                        replyMarkup: _myKeyboardMarkup.CreateOnlyLocationKeyboardMarkup(),cancellationToken:cancellationToken);
                    break;
                
                // Если он укомплектован =>
                case var s when s == "ComleatedUser":
                    
                    // Проверяем есть ли у него установленные уведомления =>
                    if (result.Item1.Notifications is null || !result.Item1.Notifications.Notify)
                    {
                        // если их нет =>
                            
                        // Бот печатает
                        await _bot.SendChatActionAsync(request.Message.From!.Id, ChatAction.Typing,cancellationToken:cancellationToken);
                        // Ответ пользователю
                        await _bot.SendTextMessageAsync(request.Message.From!.Id, 
                            "\ud83e\uddd1\u200d\ud83d\ude92 <b>Выберите время в часовом поясе (МСК UTC +3)," +
                            " когда мне отправлять вам уведомление</b>",0,ParseMode.Html,
                            replyMarkup: _myKeyboardMarkup.InlineNotificationKeyboardMarkup(),cancellationToken:cancellationToken);    
                    }
                    // Проверяем его локацию
                    else if (result.Item1.Notifications.Notify && 
                             (Regex.IsMatch(result.Item1.Location!, @"^\d+(\.\d+)?,\d+(\.\d+)?$") 
                              || Regex.IsMatch(result.Item1.Location, @"^\d+(\,\d+)?,\d+(\,\d+)?$")) )
                    {
                        
                        // При выключении приложения планировщик дропается по этому будем проверять есть ли сейчас задача с таким id
                        // Если успех то создаем ключ задачи 
                        var jobKey = new JobKey($"job{request.Message.From!.Id}", "group1");
                        Log.Information("Проверка есть ли задача с таким id");
                        if (await _scheduler.CheckExists(jobKey))
                        {
                            // Отправялем запрос на получения данных геолокации
                            var resultLocation  = _locationAccessor.GetLocation(result.Item1.Location, cancellationToken);
                            // Сохраняем данные в переменную
                            location = resultLocation.Result.Item3.DisplayName;
                            // Бот печатает
                            await _bot.SendChatActionAsync(request.TelegramId, ChatAction.Typing,cancellationToken:cancellationToken);
                            // Ответ пользователю
                            await _bot.SendTextMessageAsync(request.TelegramId, 
                                $"<b>\ud83e\uddd1\u200d\ud83d\ude92 Уведомление о погоде ежедневно в {result.Item1.Notifications!.TimeNotifications}" +
                                $" AM (МСК UTC +3) по адресу {location}</b>",
                                0, ParseMode.Html, replyMarkup: _myKeyboardMarkup.InlineCancelNotifyKeyboardMarkup(),cancellationToken:cancellationToken);
                        }
                        else // Если нет, то обновляем задачу =>
                        {
                            // Бот печатает
                            await _bot.SendChatActionAsync(request.TelegramId, ChatAction.Typing,cancellationToken:cancellationToken);
                            // Ответ пользователю
                            await _bot.SendTextMessageAsync(request.TelegramId, 
                                $"<b>\ud83d\udc77 Прошу прощения, видимо приложение выключалось," +
                                $" пожалуйста обновите свои уведомления</b>",
                                0, ParseMode.Html, replyMarkup: _myKeyboardMarkup.InlineUpdateNotifyKeyboardMarkup(),cancellationToken:cancellationToken);
                        }
                    }
                    else // Если в свойстве Location находятся не координаты, а название города =>
                    {
                        // При выключении приложения планировщик дропается по этому будем проверять есть ли сейчас задача с таким id
                        // Если успех то создаем ключ задачи 
                        var jobKey = new JobKey($"job{request.Message.From!.Id}", "group1");
                        Log.Information("Проверка есть ли задача с таким id");
                        if (await _scheduler.CheckExists(jobKey))
                        {
                            // Отправялем запрос на получения данных геолокации
                            var resultLocation  = _locationAccessor.GetLocation(result.Item1.Location, cancellationToken);
                            // Сохраняем данные в переменную
                            location = resultLocation.Result.Item3.DisplayName;
                            // Бот печатает
                            await _bot.SendChatActionAsync(request.TelegramId, ChatAction.Typing,cancellationToken:cancellationToken);
                            // Ответ пользователю
                            await _bot.SendTextMessageAsync(request.TelegramId, 
                                $"<b>\ud83e\uddd1\u200d\ud83d\ude92 Уведомление о погоде ежедневно в {result.Item1.Notifications!.TimeNotifications}" +
                                $" AM (МСК UTC +3) по адресу {location}</b>",
                                0, ParseMode.Html, replyMarkup: _myKeyboardMarkup.InlineCancelNotifyKeyboardMarkup(),cancellationToken:cancellationToken);
                        }
                        else // Если нет, то обновляем задачу =>
                        {
                            // Бот печатает
                            await _bot.SendChatActionAsync(request.TelegramId, ChatAction.Typing,cancellationToken:cancellationToken);
                            // Ответ пользователю
                            await _bot.SendTextMessageAsync(request.TelegramId, 
                                $"<b>\ud83d\udc77 Прошу прощения, видимо приложение выключалось," +
                                $" пожалуйста обновите свои уведомления</b>",
                                0, ParseMode.Html, replyMarkup: _myKeyboardMarkup.InlineUpdateNotifyKeyboardMarkup(),cancellationToken:cancellationToken);
                        }
                    }
                    break;
            }
            return Unit.Value;
        }
    }
}