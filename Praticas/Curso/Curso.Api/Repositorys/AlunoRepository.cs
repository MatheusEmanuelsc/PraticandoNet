using Curso.Api.Context;
using Curso.Api.Models;

namespace Curso.Api.Repositorys
{
    public class AlunoRepository : Repository<Aluno>, IAlunoRepository
    {
        public AlunoRepository(AppDbContext context) : base(context)
        {
        }

        
    }
}
