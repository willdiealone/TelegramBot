using Application.Interfaces;
using Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;

namespace Application.UpdateTypeCallbackQuerys.NotificationsQuerys;

public sealed class Notify 
{
    public class Request : IRequest<Unit>
    {
        // id пользователя
        public long TelegramId { get; set; }
        public string CallbackQueryId { get; set; }
        
        // Время выбранное пользователем
        public string Time { get; set; }
    }

    public class Handler : IRequestHandler<Request, Unit>
    {
        // Контекст базы данных
        private readonly DataContext _dataContext;

        // Инстанс бота
        private readonly ITelegramBotClient _bot;

        // Доступ к локаци
        private readonly ILocationAccessor _locationAccessor;
        
        // Доступ к кастомной клавиатуре
        private readonly IMyKeyboardMarkup _myKeyboardMarkup;
        
        // Констструктор класса
        public Handler(DataContext dataContext, ITelegramBotClient bot, ILocationAccessor locationAccessor, IMyKeyboardMarkup myKeyboardMarkup)
        {
            _dataContext = dataContext;
            _bot = bot;
            _locationAccessor = locationAccessor;
            _myKeyboardMarkup = myKeyboardMarkup;
        }
        
        public async Task<Unit> Handle(Request request, CancellationToken cancellationToken)
        {
            // Получаем инлайн кнопки для подтверждения
           var keyboardMarkup = _myKeyboardMarkup.InlineConfirmKeyboardMarkup();
           // Получаем пользователя из бд по id
           var user = await _dataContext.Users.Include(user => user.Notifications)
               .FirstOrDefaultAsync(u => u.UserId == request.TelegramId);
           // Опправляем запрос для получения геолокации пользователя
            var result = await _locationAccessor.GetLocation(user.Location,cancellationToken);
            // Проверем уведомления пользователя =>
            if (result.Item1)
            {
                // Если их нет, создаем, инициализируем класс и его свойтсва =>
                if (user.Notifications is null)
                {
                    Notification notification = new Notification();
                    notification.Notify = true;
                    notification.TimeNotifications = request.Time;

                    user.Notifications = notification;
                    
                    // Сохраняемя данные в бд
                    var seccess = await _dataContext.SaveChangesAsync() > 0;
                    // Если успешно =>
                    if (seccess)
                    {
                        // Возвращаем ответ
                        await _bot.AnswerCallbackQueryAsync(request.CallbackQueryId, cancellationToken: cancellationToken);
                        await _bot.SendTextMessageAsync(request.TelegramId, 
                            $"\ud83e\uddd1\u200d\ud83d\ude92 <b>Уведомление о погоде ежедневно в " +
                            $"{request.Time} AM (МСК UTC +3) по адресу {result.Item3.DisplayName}!</b>",
                            0,ParseMode.Html,replyMarkup:keyboardMarkup);    
                    }
                }
                
                // Если они не null, но их нет, то просто переопределяем свойства =>
                else if (user.Notifications.Notify == false)
                {
                    user.Notifications.Notify = true;
                    user.Notifications.TimeNotifications = request.Time;
                    // Сохраняемя данные в бд
                    var seccess = await _dataContext.SaveChangesAsync() > 0;
                    // Если успешно =>
                    if (seccess)
                    {
                        // Возвращаем ответ
                        await _bot.AnswerCallbackQueryAsync(request.CallbackQueryId, cancellationToken: cancellationToken);
                        await _bot.SendTextMessageAsync(request.TelegramId, 
                            $"\ud83e\uddd1\u200d\ud83d\ude92 <b>Уведомление о погоде ежедневно в " +
                            $"{request.Time} AM (МСК UTC +3) по адресу {result.Item3.DisplayName}!</b>",
                            0,ParseMode.Html,replyMarkup:keyboardMarkup);    
                    }
                }
                // Если они true =>
                else if (user.Notifications.Notify)
                {
                    // Возвращаем ответ
                    await _bot.AnswerCallbackQueryAsync(request.CallbackQueryId, cancellationToken: cancellationToken);
                    await _bot.SendTextMessageAsync(request.TelegramId, 
                        $"\ud83e\uddd1\u200d\ud83d\ude92 <b>Уведомление о погоде ежедневно в " +
                        $"{request.Time} AM (МСК UTC +3) по адресу {result.Item3.DisplayName}!</b>",
                        0,ParseMode.Html,replyMarkup:keyboardMarkup);
                }
                
            }
            return Unit.Value;
        }
    }
}