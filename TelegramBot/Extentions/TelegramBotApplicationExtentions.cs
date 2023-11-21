using Application.Core;
using Application.Interfaces;
using Application.UpdateTypeCommands.Commands;
using Infrastructure.TeleramBot;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Persistence;
using Quartz;
using Quartz.Impl;
using Telegram.Bot;

namespace TelegramBot.Extentions;

/// <summary>
/// Класс для добавления сервисов телеграм бота
/// </summary>
public static class TelegramBotApplicationServiceExtentions
{
    /// <summary>
    /// Метод добавляет сервисы
    /// </summary>
    /// <param name="service">коллукция сервисов</param>
    /// <param name="configutation">конфигурация</param>
    /// <returns></returns>
    public static IServiceCollection AddTelegramBotClientExtentions(this IServiceCollection service,
        IConfiguration configutation)
    {
        // Регистрируем медиатр
        service.AddMediatR(typeof(TelegramBotCommandStart.Handler).Assembly);
        service.AddSingleton<ITelegramBotAccessor, TelegramBotAccessor>();
        // Регистрируем зависимость для иснтанса телеграм бота
        service.AddSingleton<ITelegramBotClient>( provider =>
        {
            var botAcessor = provider.GetRequiredService<ITelegramBotAccessor>();
            if (botAcessor is TelegramBotAccessor accessor)
            {
                return accessor.CreateBotClient(accessor.BotClient);
            }
            return null;    
        });
        service.AddSingleton<ISchedulerFactory, StdSchedulerFactory>();
        service.AddSingleton(provider =>
        {
            ISchedulerFactory schedulerFactory = provider.GetRequiredService<ISchedulerFactory>();
            return schedulerFactory.GetScheduler().Result;
        });
        service.AddHostedService<QuartzHostedService>();
        // Регистриуем создание планировщика 
        service.AddSingleton<IMyJob, MyJob>();
        // Регистрация автомаппера.
        service.AddAutoMapper(typeof(MappingProfiles).Assembly);
        // Регистрируем сервис конфигурации.
        service.AddSingleton(configutation);
        // Сервис создания клавитуры 
        service.AddSingleton<IMyKeyboardMarkup, MyKeyboardMarkup>();
        // Регистрируем службу для доступа к WeatherAccessor.
        service.AddSingleton<IWeatherApiClientAccessor, WeatherApiClientAccessor>();
        // Добавляет IHttpClientFactory и связанные службы в IServiceCollection.
        service.AddHttpClient();
        // Регистрируем строку подлючения к бд.
        service.AddDbContext<DataContext>();
        // Ругистрируем доступ к юзери из бд.
        service.AddSingleton<IUserAccessor, UserAccessor>();
        // Регистрируем сервис геолокации.
        service.AddSingleton<ILocationAccessor, LocationAccessor>();
        // Регистрируем сервис эмодзиков
        service.AddSingleton<IEmojis, Emojis>();

        // Возвращаем коллекцию сервисов.
        return service;
    }
}