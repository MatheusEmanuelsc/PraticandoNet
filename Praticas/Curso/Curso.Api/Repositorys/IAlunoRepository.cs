using Curso.Api.Models;

namespace Curso.Api.Repositorys
{
    public interface IAlunoRepository : IRepository<Aluno>
    {


        Task<IEnumerable<Aluno>> GetAlunoPorDisciplinaAsync(int id);
    }
    
}
