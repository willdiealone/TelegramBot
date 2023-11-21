using Telegram.Bot.Types.ReplyMarkups;

namespace Application.Interfaces;

public interface IMyKeyboardMarkup
{
    /// <summary>
    /// Метод создает клавиатуру для пользователя без плана
    /// отсутствует погода на завтра, на 5 дней, уведомления, геолокация
    /// </summary>
    /// <returns></returns>
    public ReplyKeyboardMarkup CreateReplyKeyboardMarkupWithoutPlan();
    
    
    /// <summary>
    /// Метод создания инлайн кнопок для подтверждения настройки уведомлений
    /// </summary>
    /// <returns></returns>
    public InlineKeyboardMarkup InlineConfirmKeyboardMarkup();
    
    /// <summary>
    /// Метод создания инлайн кнопок для уведомления пользователя о погое
    /// </summary>
    /// <returns></returns>
    public InlineKeyboardMarkup InlineNotificationKeyboardMarkup();

    /// <summary>
    /// Метод создает инлайн кнопку для оплаты премиум плана
    /// </summary>
    /// <returns></returns>
    public InlineKeyboardMarkup InnlineChosePremiumPlanKeyboardMarkup();

    /// <summary>
    /// Метод кнопки обновления уведомлений еси приложение дроаплось
    /// </summary>
    /// <returns></returns>
    public InlineKeyboardMarkup InlineUpdateNotifyKeyboardMarkup();
    
    /// <summary>
    /// Инлайн кнопка оплаты
    /// </summary>
    /// <returns></returns>
    public InlineKeyboardMarkup InlinePaymentKeyboardMarkup(string url);
    
    /// <summary>
    /// Метод создания инлайн кнопоки поделиться ботом
    /// </summary>
    /// <returns></returns>
    public InlineKeyboardMarkup InlineShareMessageOrBot();
    
    /// <summary>
    /// Метод создания кнопки отмены уведомлений
    /// </summary>
    /// <returns></returns>
    public InlineKeyboardMarkup InlineCancelNotifyKeyboardMarkup();
    
    /// <summary>
    /// Метод создания дефолтных конопок
    /// </summary>
    /// <returns>Представляет пользовательскую клавиатуру с возможностью ответа.</returns>
    public ReplyKeyboardMarkup CreateReplyKeyboardMarkupWithFullPlan();
    
    /// <summary>
    /// Метод создания одной копки геолокации
    /// </summary>
    /// <returns>Представляет пользовательскую кнопку.</returns>
    public KeyboardButton CreateOnlyLocationButton();

    /// <summary>
    /// Метод создает полноценную клавиатуру из одной кнопки для отправики геолокации
    /// </summary>
    /// <returns></returns>
    public ReplyKeyboardMarkup CreateOnlyLocationKeyboardMarkup();

}