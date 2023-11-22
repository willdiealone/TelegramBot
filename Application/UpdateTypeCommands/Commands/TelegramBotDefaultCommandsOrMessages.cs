using Application.Interfaces;
using Domain;
using MediatR;
using Persistence;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Application.UpdateTypeCommands.Commands;

public sealed class TelegramBotDefaultCommandsOrMessages
{
    public sealed class Request : IRequest<Unit>
    {
        public long TelegramUserId { get; set; }
        public Message Message { get; set; }
    }
    
    public class Handler : IRequestHandler<Request, Unit>
    {
        /// <summary>
        /// Прокидываем экземпляр бота
        /// </summary>
        private readonly ITelegramBotClient _bot;
        
        private readonly IMyKeyboardMarkup _keyboardMarkup;
        
        /// <summary>
        /// Доступ к пользоваетелю из бд
        /// </summary>
        private readonly IUserAccessor _userAccessor;
        
        /// <summary>
        /// Контекст базы данных
        /// </summary>
        private readonly DataContext _dataContext;

        /// <summary>
        /// Доступк к сервису доступа геолокации
        /// </summary>
        private readonly ILocationAccessor _locationAccessor;
        
        public Handler(ITelegramBotClient bot,
            IUserAccessor userAccessor,  ILocationAccessor locationAccessor, DataContext dataContext, IMyKeyboardMarkup keyboardMarkup)
        {
            _bot = bot;
            _userAccessor = userAccessor;
            _locationAccessor = locationAccessor;
            _dataContext = dataContext;
            _keyboardMarkup = keyboardMarkup;
        }
        
        public async Task<Unit> Handle(Request request, CancellationToken cancellationToken)
        {
           
            // Находим пользователя в бд по id
            var result = await _userAccessor.FindOrAddUserInDb(request.Message);
            // Создаем клавиатуру на основе плана пользователя
            var keyboard = result.Item1.Plan is null ? _keyboardMarkup.CreateReplyKeyboardMarkupWithoutPlan() :
                result.Item1.Plan.PlansName == "Premium" ? _keyboardMarkup.CreateReplyKeyboardMarkupWithFullPlan() :
                _keyboardMarkup.CreateReplyKeyboardMarkupWithoutPlan();
            
            switch (result.Item2)
            {
                case { } s when s == "WaitingForCityOrLocation" :

                    var success = await _locationAccessor.GetLocation(request.Message.Text, cancellationToken);
                    if (!success.Item1)
                    {
                        await _bot.SendChatActionAsync(request.Message.From!.Id, ChatAction.Typing);
                        await _bot.SendTextMessageAsync(request.Message.From!.Id, 
                            "\ud83e\uddd1\u200d\ud83d\udd27 <b>Ой! Ошибка. Мне не удалось определить Ваше местоположение." +
                            " Пожалуйста, уточните адрес или отправьте геопозицию</b>",0,ParseMode.Html,
                            replyMarkup: _keyboardMarkup.CreateOnlyLocationKeyboardMarkup());
                        break;
                    }
                    result.Item1.Location = success.Item2;
                    result.Item1.State = UserState.ComleatedUser;
                        await _dataContext.SaveChangesAsync();
                        await _bot.SendChatActionAsync(request.Message.From!.Id, ChatAction.Typing);
                        await _bot.SendTextMessageAsync(request.Message.From!.Id, 
                            $"\ud83d\udc81\u200d\u2642\ufe0f <b>Вы выбрали {success.Item3.DisplayName}</b>",
                            0,ParseMode.Html,replyMarkup: keyboard);   
                    break;
                
                case { } s when s == "ComleatedUser" :
                    await _bot.SendChatActionAsync(request.Message.From!.Id, ChatAction.Typing);
                    await _bot.SendTextMessageAsync(request.Message.From!.Id, 
                        "<b>Я не знаю таких команд</b> \ud83e\udd37\u200d\u2642\ufe0f",0,ParseMode.Html,
                        replyMarkup: keyboard);
                    break;
                
                case { } s when s == "TemporaryLocation":
                    await _bot.SendChatActionAsync(request.Message.From!.Id, ChatAction.Typing);
                    await _bot.SendTextMessageAsync(request.Message.From!.Id, 
                        "\ud83e\uddd1\u200d\ud83d\udd27 <b>Нужно нажать на кнопку!</b>",0,ParseMode.Html,
                        replyMarkup: _keyboardMarkup.CreateOnlyLocationKeyboardMarkup());
                    break;
                    
            }
            return Unit.Value;
        }
    }
}