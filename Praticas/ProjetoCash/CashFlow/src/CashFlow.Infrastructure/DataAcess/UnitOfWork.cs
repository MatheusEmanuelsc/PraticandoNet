using CashFlow.Domain.Repositories;

namespace CashFlow.Infrastructure.DataAcess;

public class UnitOfWork : IUnitOfWork
{
    private readonly CashFlowDbContext _context;

    public UnitOfWork(CashFlowDbContext context)
    {
        _context = context;
    }
    public async Task Commit()
    {
       await _context.SaveChangesAsync();
    }
}