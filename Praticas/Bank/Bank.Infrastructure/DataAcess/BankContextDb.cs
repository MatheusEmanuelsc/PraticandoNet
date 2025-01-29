using Bank.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Bank.Infrastructure.DataAcess;

public class BankContextDb : DbContext
{
    
    public BankContextDb(DbContextOptions<BankContextDb> options) : base(options) { }

    public DbSet<Account>Accounts { get; set; }
    public DbSet<Customer>Customers { get; set; }
    public DbSet<Transaction>Transactions { get; set; }
    
    
    
}