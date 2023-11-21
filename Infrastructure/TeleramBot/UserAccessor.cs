using Application.Interfaces;
using Domain;
using Microsoft.EntityFrameworkCore;
using Persistence;
using Telegram.Bot.Types;
using User = Domain.User;

namespace Infrastructure.TeleramBot;

public class UserAccessor : IUserAccessor
{
    private readonly DataContext _dataContext;

    public UserAccessor(DataContext dataContext)
    {
        _dataContext = dataContext;
    }
    
    public async Task<(User,string)> FindOrAddUserInDb(Message message)
    {
        var user = await _dataContext.Users.Include(u=>u.Plan).Include(u=>u.Notifications).FirstOrDefaultAsync(u => u.UserId == message.From.Id);
        
        string result = String.Empty;
        switch (user)
        {
            // Если пользоваетеля нет в базе
            case null: 
                user = new()
                {
                    UserId = message.From!.Id,
                    UserName = message.From!.Username,
                    State = UserState.WaitingForCityOrLocation
                };

                // Добавляем нового пользователя в бд
                await _dataContext.AddAsync(user);
                // Сохраняем именениня в бд
                var success = await _dataContext.SaveChangesAsync() > 0;
                if (success) result = "WaitingForCityOrLocation";
                break;
            
            
            // Если пользоваетль в статусе WaitingForCityOrLocation 
            case { State:  UserState.WaitingForCityOrLocation}:
                result = "WaitingForCityOrLocation";
                break;
            
            // Если пользоваетль в статусе ComleatedUser 
            case { State:  UserState.ComleatedUser}:
                result = "ComleatedUser";
                break;
            // Если пользоваетль в статусе TemporaryLocation 
            case {State: UserState.TemporaryLocation}:
                result = "TemporaryLocation";
                break;
        }
        return (user,result);
    }
}