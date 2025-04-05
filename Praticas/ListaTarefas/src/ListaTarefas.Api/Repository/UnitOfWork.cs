using ListaTarefas.Api.Context;

namespace ListaTarefas.Api.Repository;

public class UnitOfWork: IUnitOfWork
{
    private readonly AppDbContext _context;

    public UnitOfWork(AppDbContext context )
    {
        _context = context;
    }
    public async Task<int> CommitAsync()
    {
       return  await _context.SaveChangesAsync();
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}