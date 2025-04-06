namespace ListaTarefas.Api.Repository;

public interface IUnitOfWork
{
    public ITarefaRepository Tarefa { get; }
    
    Task<int> CommitAsync();
    
}