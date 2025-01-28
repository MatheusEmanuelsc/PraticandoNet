namespace Bank.Domain.Entities;

public class Customer
{
    public long Id { get; set; }
    public string Name { get; set; }= string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    
    public ICollection<Account> Accounts { get; set; } = new List<Account>();
}