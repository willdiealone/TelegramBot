
namespace Domain;

public class Notification
{
    // id задачи
    public Guid Id { get; set; }
    // Флаг есть ли уведомления у пользователя
    public bool Notify { get; set; }
    // Время установ. пользователем для уведомлений
    public string TimeNotifications { get; set; }
    public ICollection<User> Users { get; set; }
}