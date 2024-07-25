
using Curso.Api.Context;

namespace Curso.Api.Repositorys
{
    public class UnitOfWork : IUnitOfWork
    {
        public IDisciplinaRepository? _disciplinaRepo;

        public IAlunoRepository? _alunoRepository;


        private readonly AppDbContext _context;

        public UnitOfWork(AppDbContext context)
        {
            _context = context;
        }


        public IDisciplinaRepository DisciplinaRepository
        {
            get
            {
                return _disciplinaRepo ??= new DisciplinaRepository(_context);
            }
        }

        public IAlunoRepository AlunoRepository
        {
            get
            {
                return _alunoRepository ??= new AlunoRepository(_context);
            }
        }
        public async Task CommitAsync()
        {
            await  _context.SaveChangesAsync();
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
