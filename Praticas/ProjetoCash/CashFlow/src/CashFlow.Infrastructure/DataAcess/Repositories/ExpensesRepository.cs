using CashFlow.Domain.Entities;
using CashFlow.Domain.Repositories.Expenses;

namespace CashFlow.Infrastructure.DataAcess.Repositories;

internal class ExpensesRepository : IExpensesRepository
{
    private readonly CashFlowDbContext _context;
    public ExpensesRepository(CashFlowDbContext dbContext)
    {
        _context = dbContext;
    }
    public void Add(Expense expense)
    {
        
        _context.Expenses.Add(expense);
        
    }
}