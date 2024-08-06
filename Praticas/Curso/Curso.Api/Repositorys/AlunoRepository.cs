using Curso.Api.Context;
using Curso.Api.Dtos;
using Curso.Api.Models;
using Curso.Api.Pagination;

namespace Curso.Api.Repositorys
{
    public class AlunoRepository : Repository<Aluno>, IAlunoRepository
    {
        public AlunoRepository(AppDbContext context) : base(context)
        {
        }

        
       

        
    }
}
