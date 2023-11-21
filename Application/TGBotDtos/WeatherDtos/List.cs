
namespace Application.TGBotDtos.WeatherDtos;

public class List
{
    public Main Main { get; set; }
    public Weather[] Weather { get; set; }
    public Wind Wind { get; set; }
    public string? dt_txt { get; set; }
    public DateTime? DateTime { get; set; }
    public string? DayOfWeek { get; set; }
}