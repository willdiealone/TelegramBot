namespace Domain;

public enum UserState
{
    Started = 0,
    WaitingForCityOrLocation = 1,
    ComleatedUser = 2,
    TemporaryLocation = 3,
    WaitingNotifications = 4,
}