using Application.UpdateTypeCommands.Commands;
using MediatR;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace Application.UpdateTypeCommands;

public static class CommandsHandler
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
    			await mediator.Send(new TelegramBotCommandStart.Request 
			    {
				    Message = message,
				    TelegramUserId = message.From!.Id 
			    });
    			break;
		    
		    case "/help":
			    await mediator.Send(new TelegramBotCommandHelp.Request
			    {
				    Message = message,
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