using AutoMapper;

namespace Application.TGBotDtos.LocationDtos;

public class LocationDto : Profile
{
    public string DisplayName { get; set; }
    public string City { get; set; }
    
}