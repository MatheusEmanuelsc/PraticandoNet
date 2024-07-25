namespace Curso.Api.Repositorys
{
    public interface IUnitOfWork : IDisposable
    {
        IDisciplinaRepository DisciplinaRepository { get; }
        IAlunoRepository AlunoRepository { get; }
        Task CommitAsync();
    }
}
