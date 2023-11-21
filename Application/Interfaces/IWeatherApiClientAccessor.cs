using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Application.TGBotDtos.WeatherDtos;

namespace Application.Interfaces;

/// <summary>
/// Интерфейс предоставляющий методы для получения данных о погоде на 5 дней
/// </summary>
public interface IWeatherApiClientAccessor
{
    /// <summary>
    /// Метод получения данных о погоде для нескольких городов
    /// </summary>
    /// <param name="city">Массив названий городов</param>
    /// <param name="cancellationToken">Токен отмены</param>
    /// <returns>Перечисление данных о погоде для нескольких городов</returns>
    public Task<List[]> GetWeatherForFiveDaysAsync(string city, CancellationToken cancellationToken);

    /// <summary>
    /// Метод для получения погодных данных для одного города сегодня
    /// </summary>
    /// <param name="city">Принимаем город из запроса</param>
    /// <param name="cancellationToken">Токен отмены</param>
    /// <returns>Данные о погоде одного города</returns>
    public Task<List[]> GetWeatherNowAsync(string city, CancellationToken cancellationToken);
    
    /// <summary>
    /// Метод для получения погодных данных для одного города на завтра
    /// </summary>
    /// <param name="city">Принимаем город из запроса</param>
    /// <param name="cancellationToken">Токен отмены</param>
    /// <returns>Данные о погоде одного города</returns>
    public Task<List[]> GetWeatherForTomowrrowAsync(string city, CancellationToken cancellationToken);
}