using APICatalogo.Context;
using APICatalogo.Models;

namespace APICatalogo.Repository
{
    public class CategoriaRepository : Repository<Categoria>, ICategoriaRepository
    {
        

        public CategoriaRepository(AppDbContext context): base(context)
        {
            
        }


       
    }
}
