using Curso.Api.Context;
using Curso.Api.Dtos;
using Curso.Api.Models;

namespace Curso.Api.Repositorys
{
    public class AlunoRepository : Repository<Aluno>, IAlunoRepository
    {
        public AlunoRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Aluno>> GetAlunoPorDisciplinaAsync(int id)
        {
            var aluno = await GetAllAsync();
            var alunoDisciplina = aluno.Where(a => a.DisciplinaId == id);
            return alunoDisciplina;
        }
        
    }
}
