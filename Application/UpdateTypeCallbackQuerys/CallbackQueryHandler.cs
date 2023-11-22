using Application.UpdateTypeCallbackQuerys.NotificationsQuerys;
using Application.UpdateTypeCallbackQuerys.Payment;
using MediatR;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace Application.UpdateTypeCallbackQuerys;

public static class CallbackQueryHandler
{
    /// <summary>
    ///  Метод обработки нажатия на инлайн-кнопки
    /// </summary>
    /// <param name="bot"></param>
    /// <param name="mediator"></param>
    /// <param name="callbackQuery"></param>
    /// <param name="serviceProvider"></param>
    public static async Task HandleCallbackQuery(IMediator mediator,CallbackQuery callbackQuery,
        IServiceProvider serviceProvider)
    {
        switch (callbackQuery.Data)
        {
            case "\u2705 Подтверждаю":
                await mediator.Send(new StartJob.Request
                {
                    CallbackQueryId = callbackQuery.Id,
                    Id = callbackQuery.From.Id,
                    ServiceProvider = serviceProvider,
                });
                break;
            case "Обновить уведомления":
                await mediator.Send(new StartJob.Request
                {
                    CallbackQueryId = callbackQuery.Id,
                    Id = callbackQuery.From.Id,
                    ServiceProvider = serviceProvider,
                });
                break;
            case "Отключить уведомления": 
                await mediator.Send(new EndJob.Request
                {
                    CallbackQueryId = callbackQuery.Id,
                    Id = callbackQuery.From.Id
                    
                });
                break;
            case "\u274c Отмена": 
                await mediator.Send(new EndJob.Request
                {
                    CallbackQueryId = callbackQuery.Id,
                    Id = callbackQuery.From.Id
                    
                });
                break;
            case "Выбрать Premium \ud83d\udd25":
                await mediator.Send(new TelegramBotPaymentByChoosePlan.Request
                {
                    CallbackQueryId = callbackQuery.Id,
                    NamePlan = "Premium \ud83d\udd25",
                    TelegramId = callbackQuery.From.Id,
                });
                break;
            default: 
                await mediator.Send(new Notify.Request
                {
                    TelegramId = callbackQuery.From.Id,
                    CallbackQueryId = callbackQuery.Id,
                    Time = callbackQuery.Data
                });
                break;
        }
    }
}