using Application.TGBotDtos.LocationDtos;

namespace Application.Interfaces;

public interface ILocationAccessor
{
    // Метод для доступа к локации
    public Task<(bool,string,LocationDto)> GetLocation(string city,CancellationToken cancellationToken);
}