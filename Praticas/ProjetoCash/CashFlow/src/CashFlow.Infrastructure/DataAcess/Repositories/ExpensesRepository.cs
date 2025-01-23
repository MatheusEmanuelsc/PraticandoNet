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
    public async Task Add(Expense expense)
    {
        
       await _context.Expenses.AddAsync(expense);
        
    }
}