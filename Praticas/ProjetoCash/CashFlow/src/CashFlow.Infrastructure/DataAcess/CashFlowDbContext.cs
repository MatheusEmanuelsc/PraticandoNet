using CashFlow.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CashFlow.Infrastructure.DataAcess;

public class CashFlowDbContext : DbContext
{
    internal DbSet<Expense> Expenses { get; set; }


    public CashFlowDbContext(DbContextOptions options) : base(options)
    {
    }
}