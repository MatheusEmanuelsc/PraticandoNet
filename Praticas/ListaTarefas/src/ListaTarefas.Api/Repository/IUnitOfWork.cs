namespace ListaTarefas.Api.Repository;

public interface IUnitOfWork
{
    Task<int> CommitAsync();
    void Dispose();
}