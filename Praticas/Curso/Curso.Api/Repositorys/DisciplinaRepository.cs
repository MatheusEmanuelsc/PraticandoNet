using Curso.Api.Context;
using Curso.Api.Models;

namespace Curso.Api.Repositorys
{
    public class DisciplinaRepository : Repository<Disciplina>, IDisciplinaRepository
    {
        public DisciplinaRepository(AppDbContext context) : base(context)
        {
        }
    }
}
