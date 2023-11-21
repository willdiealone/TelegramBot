namespace Domain;

public sealed class Plan
{
    public Guid Id { get; set; }

    public string PlansName { get; set; }
    
    public Decimal PlanAmount { get; set; }

    public DateTime CreateAt { get; set; }
    
    public Guid Label { get; set; }

    public ICollection<User> Users { get; set; }
}