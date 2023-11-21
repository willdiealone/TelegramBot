using System.Globalization;
using Application.TGBotDtos.LocationDtos;
using Application.TGBotDtos.WeatherDtos;
using AutoMapper;


namespace Application.Core;

public class MappingProfiles : Profile
{
    public MappingProfiles()
    {
        CreateMap<List, WeatherDto>()
            // Температура
            .ForMember(d => d.Temperature,
                o => o.MapFrom(i =>
                    Math.Round(double.Parse(i.Main.temp,CultureInfo.InvariantCulture),0)))
            // Дата
            .ForMember(d => d.Date,o => o.MapFrom(i =>
                i.DateTime.Value.Date.ToString("dd.MM")))
            // Время
            .ForMember(d => d.Time,o => o.MapFrom(i =>
                i.DateTime.Value.ToString("HH:mm")))
            // Влажность
            .ForMember(d => d.Humidity,
                o => o.MapFrom(i =>
                    i.Main.humidity))
            // Скорость
            .ForMember(d => d.Speed,
                o => o.MapFrom(i =>
                    Math.Round(double.Parse(i.Wind.Speed, CultureInfo.InvariantCulture),1)))
            // Давление
            .ForMember(d => d.Pressure, o => o.MapFrom(i =>
                i.Main.pressure))
            // Ощущается как
            .ForMember(d => d.FeelsLike, o => o.MapFrom(i =>
                Math.Round(double.Parse(i.Main.feels_like,CultureInfo.InvariantCulture),0)))
            // Описание погоды (легкий дождь)
            .ForMember(d => d.Description,
                o => o.MapFrom(i =>
                    i.Weather[0].Description))
            // Название погоды
            .ForMember(d => d.Image,
                o => o.MapFrom(i =>
                    i.Weather[0].Main));
        
        CreateMap<Location, LocationDto>()
            .ForMember(d => d.DisplayName, o => o.MapFrom(l => l.display_name))
            .ForMember(d => d.City, o => o.MapFrom(l => l.Address == null ? l.name : l.Address.city));
    }
}