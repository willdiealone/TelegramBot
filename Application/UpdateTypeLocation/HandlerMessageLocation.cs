using Application.Interfaces;
using Domain;
using MediatR;
using Persistence;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Application.UpdateTypeLocation;

public class HandlerMessageLocation 
{
     public sealed class Query : IRequest<Unit>
    {
        // id пользователя
        public long TelegramUserId { get; set; }
        
        // Сообщение пользователя
        public Message Message { get; set; }
        
        // Текущая локация пользователчя
        public string Location { get; set; }
    }
    
    public sealed class Handler : IRequestHandler<Query, Unit>
    {
        // Иснтанс телеграм бота
        private readonly ITelegramBotClient _bot;
        
        // Доступ к кастомной клавиатуре
        private readonly IMyKeyboardMarkup _keyboardMarkup;
        
        // Доступ к пользоваетелю из бд
        private readonly IUserAccessor _userAccessor;

        // Доступ к геолокации пользователя
        private readonly ILocationAccessor _locationAccessor;

        // Контекст базы данных
        private readonly DataContext _dataContext;
        
        public Handler(ITelegramBotClient bot, IUserAccessor userAccessor,
            ILocationAccessor locationAccessor,DataContext dataContext, IMyKeyboardMarkup keyboardMarkup)
        {
            _bot = bot;
            _userAccessor = userAccessor;
            _locationAccessor = locationAccessor;
            _dataContext = dataContext;
            _keyboardMarkup = keyboardMarkup;
        }
        
        public async Task<Unit> Handle(Query request, CancellationToken cancellationToken)
        {
            // Находим пользователя в бд по id
            var result = await _userAccessor.FindOrAddUserInDb(request.Message);
            // Создаем клавиатуру в зависимости от плана пользователя
            var keyboard = result.Item1.Plan is null ? _keyboardMarkup.CreateReplyKeyboardMarkupWithoutPlan() :
                result.Item1.Plan.PlansName == "Premium" ? _keyboardMarkup.CreateReplyKeyboardMarkupWithFullPlan() :
                _keyboardMarkup.CreateReplyKeyboardMarkupWithoutPlan();
            
            // Проверяме состояние пользователя
            switch (result.Item2)
            {
                // Если ожидает локацию =>
                case { } s when s == "WaitingForCityOrLocation" :
                    // Отправляем запрос для получения геоданных
                    var success = await _locationAccessor.GetLocation(request.Location,cancellationToken);
                    // Подставляем данные и возвращаем сообщение пользователю с результатом
                    if (success.Item1)
                    {
                        result.Item1.Location = success.Item2;
                        result.Item1.State = UserState.ComleatedUser;
                        await _dataContext.SaveChangesAsync();
                        await _bot.SendChatActionAsync(request.Message.From!.Id, ChatAction.Typing);
                        await _bot.SendTextMessageAsync(request.Message.From!.Id, 
                            $"\ud83d\udc81\u200d\u2642\ufe0f <b>Вы выбрали {success.Item3.DisplayName}</b>",
                            0,ParseMode.Html,
                            replyMarkup: keyboard);
                    }
                    else // Если данных нет =>
                    {
                        await _bot.SendChatActionAsync(request.Message.From!.Id, ChatAction.Typing);
                        await _bot.SendTextMessageAsync(request.Message.From!.Id, 
                            "\ud83e\uddd1\u200d\ud83d\udd27 <b>Ой! Ошибка. Мне не удалось определить Ваше местоположение." +
                            " Пожалуйста, уточните адрес или отправьте геопозицию</b>",0,ParseMode.Html,
                            replyMarkup: _keyboardMarkup.CreateOnlyLocationKeyboardMarkup());    
                    }
                    break;
                
                // Если не ожидает локацию =>
                case { } s when s == "ComleatedUser" :
                    // Отправляем запрос для получения геоданных
                    success = await _locationAccessor.GetLocation(request.Location, cancellationToken);
                    // Подставляем данные и возвращаем сообщение пользователю с результатом
                    if (success.Item1)
                    {
                        result.Item1.Location = success.Item2;
                        await _dataContext.SaveChangesAsync();
                        await _bot.SendChatActionAsync(request.Message.From!.Id, ChatAction.Typing);
                        await _bot.SendTextMessageAsync(request.Message.From!.Id, 
                            $"\ud83d\udc81\u200d\u2642\ufe0f <b>Вы выбрали {success.Item3.DisplayName}</b>",
                            0,ParseMode.Html,
                            replyMarkup: keyboard);
                    }
                    else // Если данных нет =>
                    {
                        await _bot.SendChatActionAsync(request.Message.From!.Id, ChatAction.Typing);
                        await _bot.SendTextMessageAsync(request.Message.From!.Id, 
                            "\ud83e\uddd1\u200d\ud83d\udd27 <b>Ой! Ошибка. Мне не удалось определить Ваше местоположение." +
                            " Пожалуйста, уточните адрес или отправьте геопозицию</b>",0,ParseMode.Html,
                            replyMarkup: _keyboardMarkup.CreateOnlyLocationKeyboardMarkup());
                    }
                    break;
                // Если ожидает временную локацию =>
                case { } s when s == "TemporaryLocation" :
                    // Отправляем запрос для получения геоданных
                    success = await _locationAccessor.GetLocation(request.Location, cancellationToken);
                    // Подставляем данные и возвращаем сообщение пользователю с результатом
                    if (success.Item1)
                    {
                        result.Item1.Location = success.Item2;
                        result.Item1.State = UserState.ComleatedUser;
                        await _dataContext.SaveChangesAsync();
                        await _bot.SendChatActionAsync(request.Message.From!.Id, ChatAction.Typing);
                        await _bot.SendTextMessageAsync(request.Message.From!.Id, 
                            $"\ud83d\udc81\u200d\u2642\ufe0f <b>Ваше местоположение: {success.Item3.DisplayName}</b>",
                            0,ParseMode.Html,
                            replyMarkup: keyboard);
                    }
                    else // Если данных нет =>
                    {
                        await _bot.SendChatActionAsync(request.Message.From!.Id, ChatAction.Typing);
                        await _bot.SendTextMessageAsync(request.Message.From!.Id, 
                            "\ud83e\uddd1\u200d\ud83d\udd27 <b>Ой! Ошибка. Мне не удалось определить Ваше местоположение." +
                            " Пожалуйста, уточните адрес или отправьте геопозицию</b>",0,ParseMode.Html,
                            replyMarkup: _keyboardMarkup.CreateOnlyLocationKeyboardMarkup());
                    }
                    break;
            }
            return Unit.Value;
        }
    }
}