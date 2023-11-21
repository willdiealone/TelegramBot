using Application.TGBot;

namespace Application.TGBotDtos.LocationDtos;
#nullable enable
public sealed class Location
{
    public string? display_name { get; set; }
    public Address? Address { get; set; }
    public string? name { get; set; }
}
