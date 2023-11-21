using System.Globalization;
using Application.Interfaces;
using Quartz;
using Serilog;

namespace Infrastructure.TeleramBot;

public class MyJob : IMyJob
{
    /// <summary>
    /// Создание задачи
    /// </summary>
    /// <param name="id"></param>
    /// <param name="time"></param>
    /// <param name="scheduler"></param>
    /// <typeparam name="T"></typeparam>
    public async Task NewJob<T>(long id, string time,IScheduler scheduler) where T : IJob
    {
        // Парсим время
        if (DateTime.TryParseExact(time, "HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime term))
        {
            // Получаем текущее UTC-время
            DateTime utcNow = DateTime.UtcNow;
            // Преобразуем время пользователя во время Москвы
            DateTime moscowTime = TimeZoneInfo.ConvertTime(term, TimeZoneInfo
                .FindSystemTimeZoneById("Europe/Moscow"), TimeZoneInfo.Utc);
            // Вычисляем разницу во времени
            TimeSpan delay = moscowTime - utcNow;
            // Если заданное время уже прошло, добавляем 24 часа, чтобы запланировать выполнение на следующий день
            if (delay < TimeSpan.Zero)
            {
                delay = delay.Add(TimeSpan.FromHours(24));
            }
            // Если успех то создаем ключ задачи 
            var jobKey = new JobKey($"job{id}", "group1");
            Log.Information("Проверка есть ли задача с таким id");
            if (await scheduler.CheckExists(jobKey))
            {
                Log.Information("Задача с идентификатором {JobKey} уже существует", jobKey);
            }
            else
            {
                Log.Information("Задачи с таким id нет");
                Log.Information("Создаем задачу");
                // Создаем задачу
                IJobDetail job = JobBuilder.Create<T>()
                    .WithIdentity($"job{id}", "group1")
                    .Build();
                Log.Information("запускаем задачу - (scheduler.Start)");
                await scheduler.Start();
                Log.Information("Создаем триггер для запуска задачи");
                // Создаем триггер для запуска задачи
                ITrigger trigger = TriggerBuilder.Create()
                    .WithIdentity($"trigger{id}", "group1")
                    .StartAt(DateTimeOffset.UtcNow.Add(delay)) // Указываем, когда начать выполнение задачи
                    .WithSimpleSchedule(x => x
                        .WithInterval(TimeSpan.FromHours(24)) // повторят каждые 24 часа
                        .RepeatForever())
                    .Build();
                Log.Information("scheduler.ScheduleJob(job, trigger)");
                await scheduler.ScheduleJob(job, trigger);
            }   
        }
    }

    /// <summary>
    /// Удаление задачи
    /// </summary>
    /// <param name="id"></param>
    /// <param name="scheduler"></param>
    public async Task StopJob(long id, IScheduler scheduler)
    {
        var jobKey = new JobKey($"job{id}", "group1");
        var triggerKey = new TriggerKey($"trigger{id}", "group1");
        await scheduler.DeleteJob(jobKey);
        await scheduler.UnscheduleJob(triggerKey);
        Log.Information($"Задача с id {id} удалена");
    }
}