using Bank.Domain.Enums;

namespace Bank.Domain.Entities;

public class Transaction
{
    
    public long Id { get; set; }

    
    public long AccountId { get; set; }
    public Account Account { get; set; } = null!;

    
    public TransactionType TransactionType { get; set; }
    public decimal Amount { get; set; } = 0.0m;
    public DateTime Date { get; set; } = DateTime.Now;

    
    public long? DestinationAccountId { get; set; }
    public Account? DestinationAccount { get; set; }
}