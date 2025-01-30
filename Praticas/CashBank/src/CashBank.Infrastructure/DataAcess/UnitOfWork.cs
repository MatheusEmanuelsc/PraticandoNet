using CashBank.Domain.Repositories;

namespace CashBank.Infrastructure.DataAcess;

public class UnitOfWork :IUnitOfWork
{
    private readonly CashBankContextDb _context;

    public UnitOfWork(CashBankContextDb context)
    {
        _context = context;
    }
    public async Task Commit()
    {
        await _context.SaveChangesAsync();
    }
}