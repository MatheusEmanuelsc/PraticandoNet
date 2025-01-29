using CashBank.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CashBank.Infrastructure.DataAcess;

public class CashBankContextDb : DbContext
{
    public CashBankContextDb(DbContextOptions<CashBankContextDb> options) : base(options)
    {
    }

    public DbSet<Account> Accounts { get; set; } = null!;
    public DbSet<Customer> Customers { get; set; } = null!;
    public DbSet<Transaction> Transactions { get; set; } = null!;
}