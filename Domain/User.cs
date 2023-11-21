namespace Domain;

public sealed class User 
{
	public long UserId { get; set; }
    public string? UserName { get; set; }
    public Guid? PlanId { get; set; }
    public Plan? Plan { get; set; }
    
    public Guid? NotificationsId { get; set; }
    public Notification? Notifications { get; set; }
    public string? Location { get; set; }
    public UserState State { get; set; }
    
   
}