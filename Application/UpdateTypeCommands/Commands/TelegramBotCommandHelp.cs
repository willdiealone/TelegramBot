using Application.Interfaces;
using MediatR;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Application.UpdateTypeCommands.Commands;

public sealed class TelegramBotCommandHelp
{
    public class Request : IRequest<Unit>
    {
        // Сообщение пользователя
        public Message Message { get; set; }
    }
    
    public class Handler : IRequestHandler<Request, Unit>
    {
        // Инстанс телеграм бота
        private readonly ITelegramBotClient _bot;
        
        private readonly IMyKeyboardMarkup _keyboardMarkup;

        // Доступ к пользоваетелю из бд
        private readonly IUserAccessor _userAccessor;
        
        public Handler(ITelegramBotClient bot,
            IUserAccessor userAccessor, IMyKeyboardMarkup keyboardMarkup)
        {
            _bot = bot;
            _userAccessor = userAccessor;
            _keyboardMarkup = keyboardMarkup;
        }
        
        public async Task<Unit> Handle(Request request, CancellationToken cancellationToken)
        {
            // Находим пользователя в бд по id
            var result = await _userAccessor.FindOrAddUserInDb(request.Message);
            // Создаем клавиатуру в зависимости от плана пользователя
            var keyboard = result.Item1.Plan is null ? _keyboardMarkup.CreateReplyKeyboardMarkupWithoutPlan() :
                result.Item1.Plan.PlansName == "Premium" ? _keyboardMarkup.CreateReplyKeyboardMarkupWithFullPlan() :
                _keyboardMarkup.CreateReplyKeyboardMarkupWithoutPlan();
            
            // Проверяем состояние пользователя
            switch (result.Item2)
            {
                // Если ожидает локацию =>
                case { } s when s == "WaitingForCityOrLocation" :
                    // Бот печатает
                    await _bot.SendChatActionAsync(request.Message.From!.Id, ChatAction.Typing);
                    // Возвращаем ответ
                    await _bot.SendTextMessageAsync(request.Message.From!.Id, 
                        "\u270d\ufe0f <b>Напишите название населенного пункта или отправьте свою геолокацию,"
                        + " чтобы я показал погоду</b>",0,ParseMode.Html,
                        replyMarkup: _keyboardMarkup.CreateOnlyLocationKeyboardMarkup());
                    break;
                
                // Если не ожидает локацию =>
                case { } s when s == "ComleatedUser" :
                    // Бот печатает
                    await _bot.SendChatActionAsync(request.Message.From!.Id, ChatAction.Typing);
                    // Возвращаем ответ
                    await _bot.SendTextMessageAsync(request.Message.From!.Id, 
                        "\ud83e\uddd1\u200d\ud83d\udd27 <b>По всем вопросам обратитесь к @arabaleevdennis</b>",
                        0,ParseMode.Html,
                        replyMarkup: keyboard);
                    break;
                
                // Если ожидает временную локацию =>
                case { } s when s == "TemporaryLocation":
                    // Бот печатает
                    await _bot.SendChatActionAsync(request.Message.From!.Id, ChatAction.Typing);
                    // Возвращаем ответ
                    await _bot.SendTextMessageAsync(request.Message.From!.Id, 
                        "\ud83e\uddd1\u200d\ud83d\udd27 <b>Нужно нажать на кнопку!</b>",0,ParseMode.Html,
                        replyMarkup: _keyboardMarkup.CreateOnlyLocationKeyboardMarkup());
                    break;
            }
            return Unit.Value;
        }
    }
}