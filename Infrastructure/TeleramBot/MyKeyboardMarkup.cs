using Application.Interfaces;
using Telegram.Bot.Types.ReplyMarkups;

namespace Infrastructure.TeleramBot;

public class MyKeyboardMarkup : IMyKeyboardMarkup
{

    public InlineKeyboardMarkup InlinePaymentKeyboardMarkup(string url)
    {
        return (new[]
        {
            new[]
            {
                InlineKeyboardButton.WithUrl("Оплатить", url)
            }
        });
    }
        
    
    public ReplyKeyboardMarkup CreateReplyKeyboardMarkupWithoutPlan()
    {
        var keyboardWithoutPlan = new ReplyKeyboardMarkup(new[]
        {
            new[]
            {
                new KeyboardButton("Погода сейчас \ud83c\udf0e"),
            },
            new[]
            {
                new KeyboardButton("Изменить локацию \ud83d\udccd")
            },
            new[]
            {
                new KeyboardButton("Подписка \ud83d\udd25"),
            }
        })
        {
            ResizeKeyboard = true
        };

        return keyboardWithoutPlan;
    }

    public InlineKeyboardMarkup InlineConfirmKeyboardMarkup()
    {
        return new[]
        {
            InlineKeyboardButton.WithCallbackData("\u2705 Подтверждаю", "\u2705 Подтверждаю"),
            InlineKeyboardButton.WithCallbackData("\u274c Отмена", "\u274c Отмена"),
        };
    }  
    
    public InlineKeyboardMarkup InlineCancelNotifyKeyboardMarkup()
    {
        return new[]
        {
            InlineKeyboardButton.WithCallbackData("Отключить уведомления", "Отключить уведомления"),
        };
    }  
    
    public InlineKeyboardMarkup InlineUpdateNotifyKeyboardMarkup()
    {
        return new[]
        {
            InlineKeyboardButton.WithCallbackData("Обновить уведомления", "Обновить уведомления"),
        };
    }  

     public InlineKeyboardMarkup InlineNotificationKeyboardMarkup()
    {
        InlineKeyboardMarkup inlineKeyboard = new(new[]
        {
            new[]
            {
                InlineKeyboardButton.WithCallbackData("00:00","00:00"),
                InlineKeyboardButton.WithCallbackData("01:00","01:00"),
                InlineKeyboardButton.WithCallbackData("02:00","02:00"),
                InlineKeyboardButton.WithCallbackData("03:00","03:00"),
                
            },
            new []
            {
                InlineKeyboardButton.WithCallbackData("04:00","04:00"),
                InlineKeyboardButton.WithCallbackData("05:00","05:00"),
                InlineKeyboardButton.WithCallbackData("06:00","06:00"),
                InlineKeyboardButton.WithCallbackData("07:00","07:00"),
            },
            new []
            {
                InlineKeyboardButton.WithCallbackData("08:00","08:00"),
                InlineKeyboardButton.WithCallbackData("09:00","09:00"),
                InlineKeyboardButton.WithCallbackData("10:00","10:00"),
                InlineKeyboardButton.WithCallbackData("11:00","11:00"),
            },
            new[]
            {
                InlineKeyboardButton.WithCallbackData("12:00","12:00"),
                InlineKeyboardButton.WithCallbackData("13:00","13:00"),
                InlineKeyboardButton.WithCallbackData("14:00","14:00"),
                InlineKeyboardButton.WithCallbackData("15:00","15:00"),
                
            },
            new []
            {
                InlineKeyboardButton.WithCallbackData("16:00","16:00"),
                InlineKeyboardButton.WithCallbackData("17:00","17:00"),
                InlineKeyboardButton.WithCallbackData("18:00","18:00"),
                InlineKeyboardButton.WithCallbackData("19:00","19:00"),
            },
            new []
            {
                InlineKeyboardButton.WithCallbackData("20:00","20:00"),
                InlineKeyboardButton.WithCallbackData("21:00","21:00"),
                InlineKeyboardButton.WithCallbackData("22:00","22:00"),
                InlineKeyboardButton.WithCallbackData("23:00","23:00"),
            }
        });
        return inlineKeyboard;
    }
     
     public ReplyKeyboardMarkup InnlineChosePremiumPlanKeyboardMarkup()
     {
         var keyboard = new ReplyKeyboardMarkup(new[]
         {
             new[]
             {
                 new KeyboardButton("Выбрать Premium \ud83d\udd25"),
                 new KeyboardButton("Погода завтра \ud83c\udf07")
             },
         })
         {
             ResizeKeyboard = true
         };

         return keyboard;
     }  

     public InlineKeyboardMarkup InlineShareMessageOrBot()
     {
         string shareText = "Я ваш личный помощник, который подскажет вам ваше местоположение," +
                            " а так же погоду которая вас интерисует в любое удобное для вас время \u23f1";
         string msg = $"\n\u261d\ufe0f\u261d\ufe0f\u261d\ufe0f - жми!!!\n{shareText}";
         // Создаем массив кнопок
         return new(new[]
         {
             // 1 линия кнопок
             new[]
             {
                 InlineKeyboardButton.WithSwitchInlineQuery("Поделиться ботом \ud83d\udc7e", 
                     msg),
             },
         });
     }

     public ReplyKeyboardMarkup CreateReplyKeyboardMarkupWithFullPlan()
     {
         var keyboard = new ReplyKeyboardMarkup(new[]
         {
             new[]
             {
                 new KeyboardButton("Погода сейчас \ud83c\udf0e"),
                 new KeyboardButton("Погода завтра \ud83c\udf07")
             },
             new[] { new KeyboardButton("Погода на 5 дней \ud83d\udcc5") },
             new[]
             {
                 new KeyboardButton("Мои уведомления \ud83d\udd14"),
                 new KeyboardButton("Изменить локацию \ud83d\udccd")
             },
             new[]
             {
                 new KeyboardButton("Подписка \ud83d\udd25"),
                 new KeyboardButton("Моя геолокация \ud83e\udd35")
             }
         })
         {
             ResizeKeyboard = true
         };

         return keyboard;
     }

     public KeyboardButton CreateOnlyLocationButton()
     {
         return new KeyboardButton("Отправить локацию \ud83d\udccd")
         {
             RequestLocation = true
         };
     }

     public ReplyKeyboardMarkup CreateOnlyLocationKeyboardMarkup()
     {
        
         var locationReplyKeyboard = new ReplyKeyboardMarkup(new[]
         {
             new[]
             {
                 new KeyboardButton("Отправить локацию \ud83d\udccd") { RequestLocation = true }
             }
         })
         {
             ResizeKeyboard = true
         };

         return locationReplyKeyboard;

     }
}