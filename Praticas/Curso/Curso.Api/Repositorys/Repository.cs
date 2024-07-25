namespace Curso.Api.Repositorys
{
    public class Repository : IRepository
    {
        protected readonly AppDbContext _context;

        public Repository(AppDbContext context){
            _context = context;
        }

        
    }
}
