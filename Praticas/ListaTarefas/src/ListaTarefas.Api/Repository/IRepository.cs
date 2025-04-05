using ListaTarefas.Api.Entities;

namespace ListaTarefas.Api.Repository;

public interface IRepository
{
    Task<IEnumerable<Tarefa>> GetALlTarefasAsync();
    Task<Tarefa?> GetTarefaByIdAsync(int id);
    Task AddTarefaAsync(Tarefa tarefa);
    void UpdateTarefa(Tarefa tarefa);
    void DeleteTarefa(Tarefa tarefa);
    
}