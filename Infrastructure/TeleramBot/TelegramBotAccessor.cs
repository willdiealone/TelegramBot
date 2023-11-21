using Application.Interfaces;
using Telegram.Bot;

namespace Infrastructure.TeleramBot;

/// <summary>
/// Класс для доступа к инстансу бота
/// </summary>
public sealed class TelegramBotAccessor : ITelegramBotAccessor
{
    /// <summary>
    /// Клиентский интерфейс для использования Telegram Bot API
    /// </summary>
    public ITelegramBotClient BotClient { get; }

    /// <summary>
    /// Метод возвращает инстанс бота
    /// </summary>
    /// <param name="botClient">интерфейс для использования Telegram Bot API</param>
    /// <returns>инстанс бота</returns>
    public TelegramBotClient CreateBotClient(ITelegramBotClient botClient)
    {
        var botToken = Environment.GetEnvironmentVariable("BOT_API");
        return new TelegramBotClient(botToken!);
    }
}