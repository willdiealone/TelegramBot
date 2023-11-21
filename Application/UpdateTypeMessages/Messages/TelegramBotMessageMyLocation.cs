using Application.Interfaces;
using Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Application.UpdateTypeMessages.Messages;

public class TelegramBotMessageMyLocation
{
    public sealed class Query : IRequest<Unit>
    {
        // id пользователя
        public long TelegramUserId { get; set; }
        
        // Сообщение пользователя
        public Message Message { get; set; }
    }
    
    public sealed class Handler : IRequestHandler<Query, Unit>
    {
        // Экземпляр телеграм бота
        private readonly ITelegramBotClient _bot;
        
        
        /// <summary>
        /// Доступ к кнопке локации
        /// </summary>
        private readonly IMyKeyboardMarkup _myKeyboardMarkup;

        // Контекст базы данных
        private readonly DataContext _dataContext;
        
        public Handler(ITelegramBotClient bot, 
             DataContext dataContext, IMyKeyboardMarkup myKeyboardMarkup)
        {
            _bot = bot;
            _dataContext = dataContext;
            _myKeyboardMarkup = myKeyboardMarkup;
        }
        

        public async Task<Unit> Handle(Query request, CancellationToken cancellationToken)
        {
            var user = await _dataContext.Users.Include(u=>u.Plan).FirstOrDefaultAsync(u => u.UserId == request.TelegramUserId);
            // Проверяем план пользователя 
            if (user.Plan is null || user.Plan.PlansName != "Premium")
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
            
            // Изменяем состояние пользователя на ожидание временной локации
            user.State = UserState.TemporaryLocation;
            // Сохраняем в базе данных
            var result = await _dataContext.SaveChangesAsync() > 0;
            // Если успешно
            if (result)
            {
                // Бот печатает
                await _bot.SendChatActionAsync(request.Message.From!.Id, ChatAction.Typing); 
                // Ответ пользователю
                await _bot.SendTextMessageAsync(request.Message.From!.Id, 
                    "\ud83e\uddd1\u200d\ud83d\udd27 <b>Нажмите на кнопку чтобы узнать свою геолокацию</b>",0,ParseMode.Html, 
                    replyMarkup: _myKeyboardMarkup.CreateOnlyLocationKeyboardMarkup());
            }
            return Unit.Value;
        }
    }
}