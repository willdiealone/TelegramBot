using System.Text.RegularExpressions;
using Application.Interfaces;
using Application.UpdateTypeCallbackQuerys.NotificationsQuerys.ModelsJob;
using MediatR;
using Persistence;
using Microsoft.EntityFrameworkCore;
using Quartz;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;

namespace Application.UpdateTypeCallbackQuerys.NotificationsQuerys;

public class StartJob 
{
    public sealed class Query : IRequest<Unit>
    {
        // id пользователя
        public long Id { get; set; }
        public string CallbackQueryId { get; set; }
        // провайдер сервисов
        public IServiceProvider ServiceProvider { get; set; }
    }

    public sealed class Handler : IRequestHandler<Query, Unit>
    {
        // Инстанс задачи
        private readonly IMyJob _myJob;
        
        // Инстанс планировщика
        private readonly IScheduler _scheduler;

        // Контекст базы данных
        private readonly DataContext _dataContext;

        // Инстанс телеграм бота
        private readonly ITelegramBotClient _bot;

        // Доступ к стандартной клавиатуре
        private readonly IMyKeyboardMarkup _myKeyboardMarkup;

        // Доступк к сервису геолокации
        private readonly ILocationAccessor _locationAccessor;
        
        // Конструктор класса
        public Handler(IMyJob myJob, IScheduler scheduler, DataContext dataContext,
            ITelegramBotClient bot,  ILocationAccessor locationAccessor, IMyKeyboardMarkup myKeyboardMarkup)
        {
            _myJob = myJob;
            _scheduler = scheduler;
            _dataContext = dataContext;
            _bot = bot;
            _locationAccessor = locationAccessor;
            _myKeyboardMarkup = myKeyboardMarkup;
        }
        
        public async Task<Unit> Handle(Query request, CancellationToken cancellationToken)
        {
            // Создаем переменную для хранения локации
            string location = String.Empty;
            // Получаем из базы данных пользователя влючая таблицу уведомления по id
            var user = await _dataContext.Users.Include(user => user.Notifications)
                .FirstOrDefaultAsync(u => u.UserId == request.Id);
            // Проверяем локацию является ли она координатами
            if (Regex.IsMatch(user.Location!, @"^\d+(\.\d+)?,\d+(\.\d+)?$")
                || Regex.IsMatch(user.Location, @"^\d+(\,\d+)?,\d+(\,\d+)?$") )
            {
                // Оправялем запрос для получения геоданных
                var result  = _locationAccessor.GetLocation(user.Location, cancellationToken);
                // Сохраняем результат в переменную
                location = result.Result.Item3.DisplayName;
            }
            // Проверяем уведомления пользоваетля =>
            switch (user.Notifications.Notify)
            {
                // Если уведомления установлены =>
                case true:
                    // Инициализируем свйоства для передачи в обработчик
                    NotificationWeatherJob.TelegramId = request.Id;
                    NotificationWeatherJob.ServiceProvider = request.ServiceProvider;
                    // Создаем задачу
                    await _myJob.NewJob<NotificationWeatherJob>(request.Id,user.Notifications.TimeNotifications,
                        _scheduler);
                    
                    // Возвращаем ответ
                    await _bot.AnswerCallbackQueryAsync(request.CallbackQueryId, cancellationToken: cancellationToken);
                    await _bot.SendTextMessageAsync(request.Id, 
                        $"<b>\ud83e\uddd1\u200d\ud83d\ude92 Уведомление о погоде ежедневно в {user.Notifications.TimeNotifications}" +
                        $" AM (МСК UTC +3) по адресу {location}</b>",
                        0, ParseMode.Html, replyMarkup: _myKeyboardMarkup.CreateReplyKeyboardMarkupWithFullPlan());
                    break;
                
                // Если уведомлений нет =>
                case  false:
                    // Возвращаем ответ
                    await _bot.AnswerCallbackQueryAsync(request.CallbackQueryId, cancellationToken: cancellationToken);
                    await _bot.SendTextMessageAsync(request.Id, "<b>\ud83e\uddd1\u200d\ud83d\ude92" + 
                      " Уведомления не подключены, чтобы их подключить перейдите в мои уведомления \ud83d\udd14</b>",
                        0, ParseMode.Html, replyMarkup: _myKeyboardMarkup.CreateReplyKeyboardMarkupWithFullPlan());
                    break;
            }
            return Unit.Value;
        }
    }
}