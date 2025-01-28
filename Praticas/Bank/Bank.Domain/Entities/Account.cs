using Bank.Domain.Enums;

namespace Bank.Domain.Entities;

public class Account
{
    public long Id { get; set; }
    
    public long CustomerId { get; set; }
    public Customer Customer { get; set; } = null!;

    public AccountType AccountType { get; set; }
    public decimal Balance { get; set; } = 0.0m; 
    
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime? UpdatedAt { get; set; }
}