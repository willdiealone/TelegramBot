using Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;
using Quartz;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;

namespace Application.UpdateTypeCallbackQuerys.NotificationsQuerys;

public class EndJob
{
    public sealed class Query : IRequest<Unit>
    {
        public long Id { get; set; }
        
        public string CallbackQueryId { get; set; }
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
        private readonly IMyKeyboardMarkup _keyboardMarkup;
        
        // Конструктор класса
        public Handler(IMyJob myJob, IScheduler scheduler, DataContext dataContext, ITelegramBotClient bot, IMyKeyboardMarkup keyboardMarkup)
        {
            _myJob = myJob;
            _scheduler = scheduler;
            _dataContext = dataContext;
            _bot = bot;
            _keyboardMarkup = keyboardMarkup;
        }
        
        public async Task<Unit> Handle(Query request, CancellationToken cancellationToken)
        {
            // Получаем пользователя из бд по id включая таблцу уведомления
            var user = await _dataContext.Users.Include(user => user.Notifications)
                .FirstOrDefaultAsync(u => u.UserId == request.Id);
            
            // Проверяем уведомления если они есть меняем флаги на противоположные изменяем в бд результат =>
            if (user.Notifications is not null && user.Notifications.Notify)
            {
                user.Notifications!.Notify = false;
                user.Notifications.TimeNotifications = null;
                var result = await _dataContext.SaveChangesAsync() > 0;
                if (result)
                {
                    // метод удаления задачи по id
                    await _myJob.StopJob(request.Id, _scheduler);    
                }   
                
                // Возвращаем ответ
                await _bot.AnswerCallbackQueryAsync(request.CallbackQueryId, cancellationToken: cancellationToken);
                await _bot.SendTextMessageAsync(request.Id, 
                    "\ud83e\uddd1\u200d\ud83d\ude92 Уведомления успешно отключены",
                    0,ParseMode.Html,replyMarkup:_keyboardMarkup.CreateReplyKeyboardMarkupWithFullPlan(),
                    cancellationToken:cancellationToken);
            }
            else // Усли уведомлений нет =>
            {
                // Возврщаем ответ
                await _bot.AnswerCallbackQueryAsync(request.CallbackQueryId, cancellationToken: cancellationToken);
                await _bot.SendTextMessageAsync(request.Id, 
                    "\ud83e\uddd1\u200d\ud83d\ude92 Уведомления успешно отключены",
                    0,ParseMode.Html,replyMarkup:_keyboardMarkup.CreateReplyKeyboardMarkupWithFullPlan(),
                    cancellationToken:cancellationToken);    
            }
            return Unit.Value;
        }
    }
}