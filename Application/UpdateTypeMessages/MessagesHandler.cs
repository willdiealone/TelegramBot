using Application.UpdateTypeCallbackQuerys.Payment;
using Application.UpdateTypeCommands.Commands;
using Application.UpdateTypeMessages.Messages;
using MediatR;
using Telegram.Bot.Types;

namespace Application.UpdateTypeMessages;

public static class MessagesHandler
{
		/// <summary>
		/// Метод обработки сообщений бота
		/// </summary>
		/// <param name="bot">инстанс бота</param>
		/// <param name="message">сообщение пользователя</param>
		/// <param name="mediator"></param>
    	public static async Task HandleMessage(Message message,IMediator mediator)
        {
        	switch (message.Text)
        	{
        		case "Погода сейчас \ud83c\udf0e":
			        TelegramBotMessageWeatherNow.Request.TelegramId = message.From!.Id;
			        TelegramBotMessageWeatherNow.Request.Message = message;
			        await mediator.Send(new TelegramBotMessageWeatherNow.Request());
        			break;
		        
        		case "Погода завтра \ud83c\udf07":
			        await mediator.Send(new TelegramBotMessageWeatherTomorrow.Request
			        {
				        Message = message,
				        TelegramId = message.From!.Id
			        });
        			break;
		        
        		case "Погода на 5 дней \ud83d\udcc5":
			        await mediator.Send(new TelegramBotMessageWeatherForFiveDays.Request
			        {
				        Message = message,
				        TelegramId = message.From!.Id
			        });
        			break;
		        
        		case "Мои уведомления \ud83d\udd14":
			        await mediator.Send(new TelegramBotMessageNotifications.Request
			        {
				        Message = message,
				        TelegramId = message.From!.Id
			        });
        			break;
		        
        		case "Подписка \ud83d\udd25":
			        await mediator.Send(new TelegramBotMessageSubscribe.Request
			        {
				        Message = message
			        });
        			break;
		        
		        case "Моя геолокация \ud83e\udd35" :
			        await mediator.Send(new TelegramBotMessageMyLocation.Request
			        {
				        Message = message,
				        TelegramUserId = message.From!.Id
			        });
        			break;
		        
		        case "Изменить локацию \ud83d\udccd":
			        await mediator.Send(new TelegramBotMessageSendLocation.Request
			        {
				        Message = message,
				        TelegramUserId = message.From!.Id
			        });
			        break;
		        
		        default: 
			        await mediator.Send(new TelegramBotDefaultCommandsOrMessages.Request
			        {
				        TelegramUserId = message.From!.Id,
				        Message = message
			        });
			        break;
        	}
        }
}