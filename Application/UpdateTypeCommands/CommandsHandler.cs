using Application.UpdateTypeCommands.Commands;
using MediatR;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace Application.UpdateTypeCommands;

public  class CommandsHandler
{
	/// <summary>
	/// Метод обработки команд бота
	/// </summary>
	/// <param name="bot">инстанс бота</param>
	/// <param name="message">сообщение пользователя</param>
	/// <param name="mediator"></param>
	public static async Task HandleCommands(ITelegramBotClient bot, Message message,IMediator mediator)
    {
    	switch (message.Text)
    	{
    		case "/start":
    			await mediator.Send(new TelegramBotCommandStart.Query 
			    {
				    Message = message,
				    TelegramUserId = message.From!.Id 
			    });
    			break;
		    
		    case "/help":
			    await mediator.Send(new TelegramBotCommandHelp.Query
			    {
				    Message = message,
				    TelegramUserId = message.From!.Id,
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