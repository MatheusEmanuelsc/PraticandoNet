using APICatalogo.Context;
using APICatalogo.Models;

namespace APICatalogo.Repository
{
    public class CategoriaRepository : ICategoriaRepository
    {
        private readonly AppDbContext _context;

        public CategoriaRepository(AppDbContext context)
        {
            _context = context;
        }


        IEnumerable<Categoria> ICategoriaRepository.GetCategorias()
        {
           return _context.Categorias.ToList();


        }
        Categoria ICategoriaRepository.GetCategoria(int id)
        {
            return _context.Categorias.FirstOrDefault(x => x.CategoriaId == id);
        }


        Categoria ICategoriaRepository.Create(Categoria categoria)
        {
            if (categoria is null)
            {
                throw new ArgumentNullException(nameof(categoria));
            }
            _context.Categorias.Add(categoria);
            _context.SaveChanges();
            return categoria;
        }
        Categoria ICategoriaRepository.Update(Categoria categoria)
        {
            if (categoria is null)
            {
                throw new ArgumentNullException(nameof(categoria));
            }
            _context.Entry(categoria).State= Microsoft.EntityFrameworkCore.EntityState.Modified;
            _context.SaveChanges();
            return categoria;
        }

        Categoria ICategoriaRepository.Delete(int id)
        {
            var categoria = _context.Categorias.Find(id);
            if (categoria is null)
            {
                throw new ArgumentNullException(nameof(categoria));
            }
            _context.Remove(categoria);
            _context.SaveChanges();
            return categoria;
        }
       
    }
}
