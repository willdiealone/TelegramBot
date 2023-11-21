using Application.Interfaces;
using MediatR;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Application.UpdateTypeMessages.Messages;

public class TelegramBotMessageSubscribe
{
    public sealed class Query : IRequest<Unit>
    {
        // Сообщение пользователя
        public Message Message { get; set; }
    }

    public sealed class Hander : IRequestHandler<Query, Unit>
    {

        // Инстанс телеграм бота
        private readonly ITelegramBotClient _bot;

        // Доступ к кастомной клавиатуре
        private readonly IMyKeyboardMarkup _myKeyboardMarkup;

        // Доступ к пользователю из бд по его телеграм id 
        private readonly IUserAccessor _userAccessor;

        // Конструктор класса
        public Hander(ITelegramBotClient bot, IMyKeyboardMarkup myKeyboardMarkup, IUserAccessor userAccessor)
        {
            _bot = bot;
            _myKeyboardMarkup = myKeyboardMarkup;
            _userAccessor = userAccessor;
        }
        
        public async Task<Unit> Handle(Query request, CancellationToken cancellationToken)
        {
            // Находим пользователя в бд по id
            var result = await _userAccessor.FindOrAddUserInDb(request.Message);
            if (result.Item1.Plan == null || result.Item1.Plan.PlansName != "Premium")
            {
                // Бот печатает
                await _bot.SendChatActionAsync(request.Message.From!.Id, ChatAction.Typing); 
                // Ответ пользователю (Описание премиум плана)
                await _bot.SendPhotoAsync(request.Message.From!.Id, 
                    photo: InputFile.FromUri("https://github.com/willdiealone/TelegramBot/blob/main/Application/Images/plan.JPG?raw=true"), 
                    caption: "<b>Премиум План \ud83d\udd25</b>\n\n" +
                         "Подписка на премиум план делает ваш опыт использования бота еще более удобным и информативным:\n\n" +
                         "<b>\ud83c\udf26\ufe0f Погода сейчас:</b>\nПолучайте текущий прогноз погоды в реальном времени, чтобы всегда знать, что вас ждет.\n\n" +
                         "<b>\ud83c\udf24\ufe0f Погода на завтра:</b>\nПланируйте свой день заранее, зная прогноз погоды на завтра.\n\n" +
                         "<b>\ud83c\udf26\ufe0f Погода на 5 дней:</b>\nПодробный прогноз на ближайшие 5 дней поможет вам быть готовыми к погодным изменениям.\n\n" +
                         "<b>\ud83c\udf0d Моя геолокация:</b>\nОпределите свою точное местоположение, чтобы получать более точные прогнозы.\n\n" +
                         "<b>\ud83d\udcc5 Мои уведомления:</b>\nНастраивайте персонализированные уведомления о погоде и получайте их в удобное для вас время.\n\n" +
                         "<b>\ud83c\udf0d Изменить локацию:</b>\nЛегко меняйте город, чтобы узнать погоду в разных местах.\n\n" +
                         "<b>\ud83d\udcc5 Уведомления:</b>\nНастраивайте уведомления и получайте актуальный прогноз погоды каждый день в выбранное вами время.\n\n" +
                         "С премиум подпиской у вас есть доступ ко всем функциям бота\n\n" +
                         "<b>Цена Премиум плана:\n20 рублей.</b>", 
                    parseMode: ParseMode.Html,replyMarkup: _myKeyboardMarkup.InnlineChosePremiumPlanKeyboardMarkup(), cancellationToken: cancellationToken);
            }
            else if (result.Item1.Plan.PlansName == "Premium")
            {
                // Бот печатает
                await _bot.SendChatActionAsync(request.Message.From!.Id, ChatAction.Typing);
                // Ответ пользователю
                await _bot.SendTextMessageAsync(request.Message.From!.Id, 
                    "<b>Текущая подписка:\n" +
                    $"{result.Item1.Plan!.PlansName} \ud83d\udd25</b>", 0, ParseMode.Html,
                    replyMarkup: _myKeyboardMarkup.CreateReplyKeyboardMarkupWithFullPlan());
            }
            return Unit.Value;
        }
    }
}