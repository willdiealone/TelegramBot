using Application.Interfaces;
using MediatR;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Application.UpdateTypeCommands.Commands;

public sealed class TelegramBotCommandStart 
{
    /// <summary>
    /// Класс для хранения и передачи параметров запроса 
    /// </summary>
    public class Request : IRequest<Unit>
    {
        public Message Message { get; set; }
        public long TelegramUserId { get; set; }
    }

    /// <summary>
    /// Класс обрабатывает вопрос пользователя
    /// </summary>
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
        
        public Handler(ITelegramBotClient bot, IUserAccessor userAccessor, IMyKeyboardMarkup keyboardMarkup)
        {
            _bot = bot;
            _userAccessor = userAccessor;
            _keyboardMarkup = keyboardMarkup;
        }

        /// <summary>
        /// Обрабатываем ответ
        /// </summary>
        /// <param name="request">параметры запроса</param>
        /// <param name="cancellationToken">токен отмены</param>
        /// <returns></returns>
        public async Task<Unit> Handle(Request request, CancellationToken cancellationToken)
        {
            // Находим пользователя в бд по id
            var result = await _userAccessor.FindOrAddUserInDb(request.Message);
            var keyboard = result.Item1.Plan is null ? _keyboardMarkup.CreateReplyKeyboardMarkupWithoutPlan() :
                result.Item1.Plan.PlansName == "Premium" ? _keyboardMarkup.CreateReplyKeyboardMarkupWithFullPlan() :
                _keyboardMarkup.CreateReplyKeyboardMarkupWithoutPlan();
            switch (result.Item2)
            {
                case { } s when s == "WaitingForCityOrLocation" :
                    await _bot.SendChatActionAsync(request.Message.From!.Id, ChatAction.Typing);
                    await _bot.SendTextMessageAsync(request.Message.From!.Id, 
                        "\u270d\ufe0f <b>Напишите название населенного пункта или отправьте свою геолокацию,"
                        + " чтобы я показал погоду</b>",0,ParseMode.Html,
                        replyMarkup: _keyboardMarkup.CreateOnlyLocationKeyboardMarkup());
                    break;
                
                case { } s when s == "ComleatedUser" :
                    await _bot.SendChatActionAsync(request.Message.From!.Id, ChatAction.Typing);
                    await _bot.SendTextMessageAsync(request.Message.From!.Id, 
                        "\ud83e\uddd1\u200d\ud83d\udd27 <b>Бот теперь активен, вам больше не нужна эта команда</b>",
                        0,ParseMode.Html,
                        replyMarkup:keyboard);
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