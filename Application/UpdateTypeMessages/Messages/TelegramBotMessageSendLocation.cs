using Application.Interfaces;
using Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Application.UpdateTypeMessages.Messages;

public class TelegramBotMessageSendLocation
{
    public sealed class Query : IRequest<Unit>
    {
        public long TelegramUserId { get; set; }
        public Message Message { get; set; }
    }
    
    public sealed class Handler : IRequestHandler<Query, Unit>
    {
        /// <summary>
        /// Прокидываем экземпляр бота
        /// </summary>
        private readonly ITelegramBotClient _bot;

        /// <summary>
        /// Контекст базы данных
        /// </summary>
        private readonly DataContext _dataContext;
        
        /// <summary>
        /// Доступ к кнопке локации
        /// </summary>
        private readonly IMyKeyboardMarkup _myKeyboardMarkup;
        
        public Handler(ITelegramBotClient bot, DataContext dataContext, IMyKeyboardMarkup myKeyboardMarkup)
        {
            _bot = bot;
            _dataContext = dataContext;
            _myKeyboardMarkup = myKeyboardMarkup;
        }
        
        public async Task<Unit> Handle(Query request, CancellationToken cancellationToken)
        {
            var user = await _dataContext.Users.FirstOrDefaultAsync(u => u.UserId == request.TelegramUserId);
            user.State = UserState.WaitingForCityOrLocation;
            var result = await _dataContext.SaveChangesAsync() > 0;
            if (result)
            {
                await _bot.SendChatActionAsync(request.Message.From!.Id, ChatAction.Typing); 
                await _bot.SendTextMessageAsync(request.Message.From!.Id, 
                    "\u270d\ufe0f <b>Напишите название населенного пункта или отправьте свою геолокацию,"
                    + " чтобы я показал погоду</b>",0,ParseMode.Html,
                    replyMarkup: _myKeyboardMarkup.CreateOnlyLocationKeyboardMarkup());
            }
            return Unit.Value;
        }
    }
}