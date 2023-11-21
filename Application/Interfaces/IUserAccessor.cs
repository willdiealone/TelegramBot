using Telegram.Bot.Types;
using User = Domain.User;

namespace Application.Interfaces;

public interface IUserAccessor
{
    // Метод изет в бд пользователя по id, если его там нет добавляет
    Task<(User,string)> FindOrAddUserInDb(Message message);
}