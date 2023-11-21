using System.Text;
using System.Text.RegularExpressions;
using Application.Interfaces;
using Application.TGBotDtos.LocationDtos;
using AutoMapper;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Location = Application.TGBotDtos.LocationDtos.Location;

namespace Infrastructure.TeleramBot;

public class LocationAccessor : ILocationAccessor
{
    /// <summary>
    /// HttpClient для отправки HTTP-запросов.
    /// </summary>
    private readonly HttpClient _httpClient;

    /// <summary>
    /// Интерфесй для работы с конфигурацией приложения.
    /// </summary>
    private readonly IConfiguration _configuration;

    /// <summary>
    /// Поле для хранения url к nominatin
    /// </summary>
    public readonly string Url;

    /// <summary>
    /// Свойство для доступа к маппингу
    /// </summary>
    private readonly IMapper _mapper;

    public LocationAccessor(HttpClient httpClient,IConfiguration configuration, IMapper mapper)
    {
        _configuration = configuration;
        _mapper = mapper;
        Url = _configuration["Nominatim:Url"];
        _httpClient = httpClient;
        _httpClient.BaseAddress = new Uri(Url!);
        _httpClient.DefaultRequestHeaders.Add("User-Agent","MyTelegramBotWeatherForecast/1.0");
    }
    
    /// <summary>
    /// Метод проверят геолокацию
    /// </summary>
    /// <param name="city"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<(bool,string,LocationDto)> GetLocation(string city,CancellationToken cancellationToken)
    {
        Location location = new();
        Location[] locationArray;
        LocationDto locationDto = new();
        HttpResponseMessage responseMessage;
        string pathToNominatim;
        // Проверяем, является ли параметр city координатами (по формату)
        if (Regex.IsMatch(city, @"^\d+(\.\d+)?,\d+(\.\d+)?$")
            || Regex.IsMatch(city, @"^\d+(\,\d+)?,\d+(\,\d+)?$"))
        {
            // Если city соответствует формату координат, используем их для запроса
            StringBuilder lat = new StringBuilder(); StringBuilder lon = new StringBuilder();
            var arr = city.Split(",",4);
            lat.Append(arr[0]); lat.Append("."); lat.Append(arr[1]);
            lon.Append(arr[2]); lon.Append("."); lon.Append(arr[3]);
            pathToNominatim = Url + $"reverse?lat={lat}&lon={lon}&format=json";
            // Отправляем GET запрос.
            responseMessage = await _httpClient.GetAsync(pathToNominatim, cancellationToken);
            // Если успешно (200)
            if (responseMessage.IsSuccessStatusCode) 
            {
                // Считываем содержимое ответа.
                var content = await responseMessage.Content.ReadAsStringAsync();
                // Считываем содержимое ответа.
                // Парсим JSON
                location = JsonConvert.DeserializeObject<Location>(content);
                // Сохраняем.
                locationDto = _mapper.Map<Location, LocationDto>(location);
            }
            else
            {
                return new(false, null, null);   
            }
        }
        else
        {
            // Формируем Url для запроса.
            pathToNominatim = Url + $"search?q={city}&format=json";
            // Отправляем GET запрос.
            responseMessage = await _httpClient.GetAsync(pathToNominatim, cancellationToken);
            // Если успешно (200)
            if (responseMessage.IsSuccessStatusCode) 
            {
                // Считываем содержимое ответа.
                var content = await responseMessage.Content.ReadAsStringAsync();
                if (!content.StartsWith("[{") && !content.EndsWith("}]"))
                {
                    return new(false, null, null);
                }
                // Парсим JSON
                locationArray = JsonConvert.DeserializeObject<Location[]>(content);
                locationDto = _mapper.Map<Location, LocationDto>(locationArray[0]);
            }
        }
        return new (true,city,locationDto);
    }
}