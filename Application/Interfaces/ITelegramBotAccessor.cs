using Telegram.Bot;
namespace Application.Interfaces;

public interface ITelegramBotAccessor
{
    // Клиентский интерфейс для использования Telegram Bot API
    public ITelegramBotClient BotClient { get; }
}