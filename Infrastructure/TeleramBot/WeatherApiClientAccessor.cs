using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using Application.Interfaces;
using Application.TGBotDtos.WeatherDtos;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace Infrastructure.TeleramBot;

/// <summary>
/// Клиент взаимодействия с API OpenWeather и получения данных о погоде
/// для нескольких городов.
/// </summary>
public sealed class WeatherApiClientAccessor : IWeatherApiClientAccessor
{
    /// <summary>
    /// Интерфесй для работы с конфигурацией приложения.
    /// </summary>
    private readonly IConfiguration _configuration;
    
    /// <summary>
    /// HttpClient для отправки HTTP-запросов.
    /// </summary>
    private readonly HttpClient _httpClient;

    /// <summary>
    /// Поле для ханнения ApiKey.
    /// </summary>
    public readonly string ApiKey;

    /// <summary>
    /// Поле для ханнения ApiUrl.
    /// </summary>
    public readonly string ApiUrl;
    
    /// <summary>
    /// Констурктор принимает IConfiguration и HttpClient.
    /// </summary>
    /// <param name="configuration"></param>
    /// <param name="httpClient"></param>
    public WeatherApiClientAccessor(IConfiguration configuration, HttpClient httpClient)
    {
        _configuration = configuration;
        ApiKey = _configuration["OpenWeather:ApiKey"];
        ApiUrl = _configuration["OpenWeather:ApiUrl"];
        _httpClient = httpClient;
        // Устанавливаем базовый Url для HttpClient.
        _httpClient.BaseAddress = new Uri(ApiUrl!);
    }

    /// <summary>
    /// Метод для получения погодных данных для одного города
    /// </summary>
    /// <param name="city">Принимаем город из запроса</param>
    /// <param name="cancellationToken">Токен отмены</param>
    /// <returns>Данные о погоде одного города</returns>
    public async Task<List[]> GetWeatherNowAsync(string city,CancellationToken cancellationToken)
    {
        string pathToOpenWeather;
        var morning = new DateTime(DateTime.Now.Year,DateTime.Now.Month,DateTime.Now.Day,09,0,0).ToString("yyyy-MM-dd HH:mm:ss");
        var day = new DateTime(DateTime.Now.Year,DateTime.Now.Month,DateTime.Now.Day,15,0,0).ToString("yyyy-MM-dd HH:mm:ss");
        var night = new DateTime(DateTime.Now.Year,DateTime.Now.Month,DateTime.Now.Day,21,0,0).ToString("yyyy-MM-dd HH:mm:ss");

        if (Regex.IsMatch(city,@"^\S+\s+\S{1,4}\.\S+$"))
            return null;
        
        if (Regex.IsMatch(city, @"^\d+(\.\d+)?,\d+(\.\d+)?$")
                 || Regex.IsMatch(city, @"^\d+(\,\d+)?,\d+(\,\d+)?$"))
        {
            // Если city соответствует формату координат, используем их для запроса
            StringBuilder lat = new StringBuilder(); StringBuilder lon = new StringBuilder();
            var arr = city.Split(",",4);
            lat.Append(arr[0]); lat.Append("."); lat.Append(arr[1]);
            lon.Append(arr[2]); lon.Append("."); lon.Append(arr[3]);
            pathToOpenWeather = ApiUrl + $"?lat={lat}&lon={lon}&appid=" + ApiKey + "&units=metric&lang=ru";
            // Отправляем GET запрос.
            var responseMessage = await _httpClient.GetAsync(pathToOpenWeather, cancellationToken);
            if (!responseMessage.IsSuccessStatusCode)
                return null;
            
            // Считываем содержимое ответа.
            var content = await responseMessage.Content.ReadAsStringAsync();
            // Парсим JSON
            // Сохраняем данные о погоде в массив weatherInfo.
            var weatherInfo = JsonConvert.DeserializeObject<WeatherInfo>(content);
            if (weatherInfo is null)
                return null;

            var array = weatherInfo.List.Where(l => l.dt_txt! == morning || l.dt_txt! == day || l.dt_txt! == night).Select(l =>
            {
                l.DateTime = DateTime.ParseExact(l.dt_txt!, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
                l.DayOfWeek = l.DateTime.Value.ToString("dddd", new CultureInfo("ru-RU")); return l;
            }).ToArray();
            return array;
        }
        else 
        {
            pathToOpenWeather = ApiUrl + $"?q={city}" + "&appid=" + ApiKey + "&lang=ru&units=metric";
            // Отправляем GET запрос.
           var responseMessage = await _httpClient.GetAsync(pathToOpenWeather, cancellationToken);
            if (!responseMessage.IsSuccessStatusCode)
                return null;
            
            // Считываем содержимое ответа.
            var content = await responseMessage.Content.ReadAsStringAsync();
            // Парсим JSON
            // Сохраняем данные о погоде в массив weatherInfo.
            var weatherInfo = JsonConvert.DeserializeObject<WeatherInfo>(content);
            if (weatherInfo is null)
                return null;
            
            var array = weatherInfo.List.Where(l => l.dt_txt! == morning || l.dt_txt! == day || l.dt_txt! == night).Select(l =>
            {
                l.DateTime = DateTime.ParseExact(l.dt_txt!, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture); return l;
            }).ToArray();
            return array;
        }
    }

    public async Task<List[]> GetWeatherForTomowrrowAsync(string city, CancellationToken cancellationToken)
    {
        string pathToOpenWeather;
        HttpResponseMessage responseMessage;
        var tomorrow = DateTime.Now.AddDays(1);
        var tomorrowMorning = new DateTime(DateTime.Now.Year,DateTime.Now.Month,tomorrow.Day,09,0,0).ToString("yyyy-MM-dd HH:mm:ss");
        var tomorrowDay = new DateTime(DateTime.Now.Year,DateTime.Now.Month,tomorrow.Day,15,0,0).ToString("yyyy-MM-dd HH:mm:ss");
        var tomorrowNight = new DateTime(DateTime.Now.Year,DateTime.Now.Month,tomorrow.Day,21,0,0).ToString("yyyy-MM-dd HH:mm:ss");
        if (Regex.IsMatch(city,@"^\S+\s+\S{1,4}\.\S+$"))
            return null;

        string content = string.Empty;
        WeatherInfo weatherInfo;
        List[] array;
        if (Regex.IsMatch(city, @"^\d+(\.\d+)?,\d+(\.\d+)?$")
            || Regex.IsMatch(city, @"^\d+(\,\d+)?,\d+(\,\d+)?$"))
        {
            // Если city соответствует формату координат, используем их для запроса
            StringBuilder lat = new StringBuilder(); StringBuilder lon = new StringBuilder();
            var arr = city.Split(",",4);
            lat.Append(arr[0]); lat.Append("."); lat.Append(arr[1]);
            lon.Append(arr[2]); lon.Append("."); lon.Append(arr[3]);
            pathToOpenWeather = ApiUrl + $"?lat={lat}&lon={lon}&appid=" + ApiKey + "&units=metric&lang=ru";
            // Отправляем GET запрос.
            responseMessage = await _httpClient.GetAsync(pathToOpenWeather, cancellationToken);
            // Если успешно (200)
            if (!responseMessage.IsSuccessStatusCode)
                return null;
            
            // Считываем содержимое ответа.
            content = await responseMessage.Content.ReadAsStringAsync();
            // Парсим JSON
            // Сохраняем данные о погоде в массив weatherInfo.
            weatherInfo = JsonConvert.DeserializeObject<WeatherInfo>(content);
            if (weatherInfo is null)
                return null;
            
            array = weatherInfo.List.Where(l => l.dt_txt! == tomorrowMorning || l.dt_txt! == tomorrowDay || l.dt_txt! == tomorrowNight).Select(l =>
            {
                l.DateTime = DateTime.ParseExact(l.dt_txt!, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
                l.DayOfWeek = l.DateTime.Value.ToString("dddd", new CultureInfo("ru-RU")); return l;
            }).ToArray();
            return array;
        }
        pathToOpenWeather = ApiUrl + $"?q={city}" + "&appid=" + ApiKey + "&lang=ru&units=metric";
        // Отправляем GET запрос.
        responseMessage = await _httpClient.GetAsync(pathToOpenWeather, cancellationToken);
        // Если успешно (200)
        if (!responseMessage.IsSuccessStatusCode)
            return null;
        
        // Считываем содержимое ответа.
        content = await responseMessage.Content.ReadAsStringAsync();
        // Парсим JSON
        weatherInfo = JsonConvert.DeserializeObject<WeatherInfo>(content);
        // Сохраняем данные о погоде в массив weatherInfo.
        if (weatherInfo is null)
            return null;

        array = weatherInfo.List.Where(l => l.dt_txt! == tomorrowMorning || l.dt_txt! == tomorrowDay || l.dt_txt! == tomorrowNight).Select(l =>
        {
            l.DateTime = DateTime.ParseExact(l.dt_txt!, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
            l.DayOfWeek = l.DateTime.Value.ToString("dddd", new CultureInfo("ru-RU")); return l;
        }).ToArray();
        
        return array;
    }

    /// <summary>
    /// Метод для получения погодных данных для нескольких городов
    /// </summary>
    /// <param name="city">Принимаем города из запроса</param>
    /// <param name="cancellationToken">Токен отмены</param>
    /// <returns>Перечисление данных о погоде для нескольких городов</returns>
    public async Task<List[]> GetWeatherForFiveDaysAsync(string city, CancellationToken cancellationToken)
    {
        string pathToOpenWeather;
        HttpResponseMessage responseMessage;
        string[] checkCity;
        if (Regex.IsMatch(city,@"^\S+\s+\S{1,4}\.\S+$"))
            return null;

        string content;
        WeatherInfo weatherInfo;
        List[] array;
        if (Regex.IsMatch(city, @"^\d+(\.\d+)?,\d+(\.\d+)?$")
            || Regex.IsMatch(city, @"^\d+(\,\d+)?,\d+(\,\d+)?$"))
        {
            // Если city соответствует формату координат, используем их для запроса
            StringBuilder lat = new StringBuilder(); StringBuilder lon = new StringBuilder();
            var arr = city.Split(",",4);
            lat.Append(arr[0]); lat.Append("."); lat.Append(arr[1]);
            lon.Append(arr[2]); lon.Append("."); lon.Append(arr[3]);
            pathToOpenWeather = ApiUrl + $"?lat={lat}&lon={lon}&appid=" + ApiKey + "&units=metric&lang=ru";
            // Отправляем GET запрос.
            responseMessage = await _httpClient.GetAsync(pathToOpenWeather, cancellationToken);
            // Если успешно (200)
            if (!responseMessage.IsSuccessStatusCode)
                return null;
            
            // Считываем содержимое ответа.
            content = await responseMessage.Content.ReadAsStringAsync();
            // Парсим JSON
            weatherInfo = JsonConvert.DeserializeObject<WeatherInfo>(content);
            // Сохраняем данные о погоде в массив weatherInfo.
            if (weatherInfo is null)
                return null;
            array = weatherInfo.List.Where(l => l.dt_txt!.EndsWith("15:00:00") || l.dt_txt!.EndsWith("21:00:00")).Select(l =>
            {
                l.DateTime = DateTime.ParseExact(l.dt_txt!, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
                l.DayOfWeek = l.DateTime.Value.ToString("dddd", new CultureInfo("ru-RU")); return l;
            }).ToArray();
            return array;
        }
        
        // Формируем Url для запроса.
        pathToOpenWeather = ApiUrl + $"?q={city}" + "&appid=" + ApiKey + "&lang=ru&units=metric";
        // Отправляем GET запрос.
        responseMessage = await _httpClient.GetAsync(pathToOpenWeather, cancellationToken);
        // Если успешно (200)
        if (!responseMessage.IsSuccessStatusCode)
            return null;
        // Считываем содержимое ответа.
        content = await responseMessage.Content.ReadAsStringAsync();
        // Парсим JSON
        weatherInfo = JsonConvert.DeserializeObject<WeatherInfo>(content);
        // Сохраняем данные о погоде в массив weatherInfo.
        if (weatherInfo is null)
            return null;
        
        array = weatherInfo.List.Where(l => l.dt_txt!.EndsWith("15:00:00") || l.dt_txt!.EndsWith("21:00:00")).Select(l =>
        {
            l.DateTime = DateTime.ParseExact(l.dt_txt!, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
            l.DayOfWeek = l.DateTime.Value.ToString("dddd", new CultureInfo("ru-RU")); return l;
        }).ToArray();
        return array;
    }
}