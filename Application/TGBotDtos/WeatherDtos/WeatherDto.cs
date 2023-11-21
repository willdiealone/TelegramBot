
namespace Application.TGBotDtos.WeatherDtos;

public sealed class WeatherDto
{
    public string Image { get; set; }
    public double Temperature { get; set; }
    public string Description { get; set; }
    public string Humidity { get; set; }
    public double Speed { get; set; }
    public double FeelsLike { get; set; }
    public double Pressure { get; set; }
    public string Date { get; set; }
    public string Time { get; set; }
    public string DayOfWeek { get; set; }
}