using Application.UpdateTypeCallbackQuerys.Payment;
using Application.UpdateTypeCommands.Commands;
using Application.UpdateTypeMessages.Messages;
using MediatR;
using Telegram.Bot.Types;

namespace Application.UpdateTypeMessages;

public static  class MessagesHandler
{
		/// <summary>
		/// Метод обработки сообщений бота
		/// </summary>
		/// <param name="bot">инстанс бота</param>
		/// <param name="message">сообщение пользователя</param>
		/// <param name="mediator"></param>
    	public  static async Task HandleMessage(Message message,IMediator mediator)
        {
        	switch (message.Text)
        	{
        		case "Погода сейчас \ud83c\udf0e":
			        TelegramBotMessageWeatherNow.Query.TelegramId = message.From!.Id;
			        TelegramBotMessageWeatherNow.Query.Message = message;
			        await mediator.Send(new TelegramBotMessageWeatherNow.Query());
        			break;
		        
        		case "Погода завтра \ud83c\udf07":
			        await mediator.Send(new TelegramBotMessageWeatherTomorrow.Query
			        {
				        Message = message,
				        TelegramId = message.From!.Id
			        });
        			break;
		        
        		case "Погода на 5 дней \ud83d\udcc5":
			        await mediator.Send(new TelegramBotMessageWeatherForFiveDays.Query
			        {
				        Message = message,
				        TelegramId = message.From!.Id
			        });
        			break;
		        
        		case "Мои уведомления \ud83d\udd14":
			        await mediator.Send(new TelegramBotMessageNotifications.Query
			        {
				        Message = message,
				        TelegramId = message.From!.Id
			        });
        			break;
		        
        		case "Подписка \ud83d\udd25":
			        await mediator.Send(new TelegramBotMessageSubscribe.Query
			        {
				        Message = message
			        });
        			break;
		        
		        case "Моя геолокация \ud83e\udd35" :
			        await mediator.Send(new TelegramBotMessageMyLocation.Query
			        {
				        Message = message,
				        TelegramUserId = message.From!.Id
			        });
        			break;
		        
		        case "Изменить локацию \ud83d\udccd":
			        await mediator.Send(new TelegramBotMessageSendLocation.Query
			        {
				        Message = message,
				        TelegramUserId = message.From!.Id
			        });
			        break;
		        
		        default: 
			        await mediator.Send(new TelegramBotDefaultCommandsOrMessages.Query
			        {
				        TelegramUserId = message.From!.Id,
				        Message = message
			        });
			        break;
        	}
        }
}