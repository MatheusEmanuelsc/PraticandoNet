using ListaTarefas.Api.Context;

namespace ListaTarefas.Api.Repository;

public class UnitOfWork: IUnitOfWork
{
    private readonly AppDbContext _context;
    public ITarefaRepository Tarefa { get; }

    public UnitOfWork(AppDbContext context , ITarefaRepository tarefaRepository)
    {
        _context = context;
        Tarefa = tarefaRepository;
    }

    
    public async Task<int> CommitAsync()
    {
       return  await _context.SaveChangesAsync();
    }

   
}