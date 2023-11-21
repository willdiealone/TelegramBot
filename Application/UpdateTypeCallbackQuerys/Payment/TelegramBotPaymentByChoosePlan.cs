using Application.Interfaces;
using Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using yoomoney_api.notification;
using yoomoney_api.quickpay;

namespace Application.UpdateTypeCallbackQuerys.Payment;

public class TelegramBotPaymentByChoosePlan
{
    public sealed class Query : IRequest<Unit>
    {
        // Название выбранного плана
        public string NamePlan { get; set; }
        // id пользователя
        public long TelegramId { get; set; }
        
        public string CallbackQueryId { get; set; }
    }

    public sealed class Handler : IRequestHandler<Query, Unit>
    {
        // Инстанс бота
        private readonly ITelegramBotClient _bot;

        // Доступ с кастомной клавиатуре
        private readonly IMyKeyboardMarkup _myKeyboardMarkup;

        private readonly DataContext _dataContext;
        
        // Констуркор класса
        public Handler(ITelegramBotClient bot, IMyKeyboardMarkup myKeyboardMarkup, DataContext dataContext)
        {
            _bot = bot;
            _myKeyboardMarkup = myKeyboardMarkup;
            _dataContext = dataContext;
        }
        public async Task<Unit> Handle(Query request, CancellationToken cancellationToken)
        {
            var label = Guid.NewGuid(); 
            var quickpay = new Quickpay(receiver: "4100118408605024", quickpayForm: "shop", sum: 10, label:label.ToString(), paymentType: "AC");


            
            // Ответ пользователю
            await _bot.AnswerCallbackQueryAsync(request.CallbackQueryId, cancellationToken: cancellationToken);
            await _bot.SendPhotoAsync(request.TelegramId,
                photo: InputFile.FromUri("https://github.com/willdiealone/TelegramBot/blob/main/Application/Images/plan.JPG?raw=true"),
                caption: "<b>Вы выбрали:\n\n" +
                $"{request.NamePlan}\n" +
                "К оплате: 20 рублей</b>",parseMode: ParseMode.Html,replyMarkup: 
                _myKeyboardMarkup.InlinePaymentKeyboardMarkup(quickpay.LinkPayment),cancellationToken: cancellationToken);
            
            PaymentListenerToYooMoney paymentListenerToYooMoney = new(label.ToString(),DateTime.Today,Environment.GetEnvironmentVariable("NOTIFICATION_SECRET"));
            string resultPayment = string.Empty;
            _ = Task.Run(async () =>
            {
                 resultPayment = await paymentListenerToYooMoney.Listen("127.0.0.1", 9000);
                
                 if (resultPayment.Contains("\nУспешно"))
                 {
                     Console.WriteLine(resultPayment);
                     var user = await _dataContext.Users.Include(u=>u.Plan).FirstOrDefaultAsync(u => u.UserId == request.TelegramId);
                     if (user.Plan is null || user.Plan.PlansName == "")
                     {
                         user.Plan = new Plan
                         {
                             PlansName = "Premium",
                             PlanAmount = 10,
                             Label = label,
                             CreateAt = DateTime.UtcNow
                         };   
                     }

                     var success = await _dataContext.SaveChangesAsync() > 0;

                     if (success)
                     {
                         // Бот печатает
                         await _bot.SendChatActionAsync(request.TelegramId, ChatAction.Typing); 
                         // Ответ пользователю
                         await _bot.SendTextMessageAsync(request.TelegramId, 
                             "\ud83e\uddd1\u200d\ud83d\udd27 <b>Подписка Premium успешно подключена</b>",
                             0,ParseMode.Html, 
                             replyMarkup: _myKeyboardMarkup.CreateReplyKeyboardMarkupWithFullPlan());
                     }
                 }
            });
            return Unit.Value;
        }
    }
}