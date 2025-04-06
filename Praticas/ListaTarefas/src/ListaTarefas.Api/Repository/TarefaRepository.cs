using ListaTarefas.Api.Context;
using ListaTarefas.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace ListaTarefas.Api.Repository;

public class TarefaRepository: ITarefaRepository
{
    private readonly AppDbContext _appDbContext;

    public TarefaRepository( AppDbContext appDbContext )
    {
        _appDbContext = appDbContext;
    }
    
    public async Task<IEnumerable<Tarefa>> GetALlTarefasAsync()
    {
        var listAsync = await _appDbContext.Tarefas.ToListAsync();
        return listAsync;
    }

    public async Task<Tarefa?> GetTarefaByIdAsync(int id)
    {
        return await _appDbContext.Tarefas.FindAsync(id);
    }

    public async Task AddTarefaAsync(Tarefa tarefa)
    { 
       await _appDbContext.Tarefas.AddAsync(tarefa);
       
    }

    public void  UpdateTarefa(Tarefa tarefa)
    { 
        _appDbContext.Tarefas.Update(tarefa);
    }

    public void  DeleteTarefa(Tarefa tarefa)
    {
        _appDbContext.Tarefas.Remove(tarefa);
    }
}