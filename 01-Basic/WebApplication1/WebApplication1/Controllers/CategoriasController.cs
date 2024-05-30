
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Context;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoriasController : ControllerBase
    {
        private readonly AppDbContext _context;

        public CategoriasController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public ActionResult<IEnumerable<Categoria>> ListaCategorias()
        {
            try
            {
                var categorias = _context.Categorias.AsNoTracking().ToList();
                if (categorias is null)
                {
                    return NotFound("Categoria Não encontrada");
                }
                return categorias;
            }
            catch (Exception)
            {

                return StatusCode(StatusCodes.Status500InternalServerError,
                                 "Ocorreu um problema ao tratar a sua solicitação.");
            }
        }


        [HttpGet("{id:int}", Name = "ObterCategoria")]
        public ActionResult<Categoria> CategoriaPorId(int id)
        {
            try
            {
                var categoria = _context.Categorias.AsNoTracking().FirstOrDefault(c => c.CategoriaId == id);

                if (categoria is null)
                {
                    return NotFound("Categoria Não encontrada");
                }
                return categoria;
            }

            catch (Exception)
            {

                return StatusCode(StatusCodes.Status500InternalServerError,
                                "Ocorreu um problema ao tratar a sua solicitação.");
            }
        }


        [HttpPost]

        public ActionResult<Categoria> CriandoCategoria(Categoria categoria)
        {
        try
        {
            _context.Categorias.Add(categoria);
            _context.SaveChanges();

            return new CreatedAtRouteResult("ObterCategoria", new { id = categoria.CategoriaId }, categoria);
        }
        catch (Exception)
        {

            return StatusCode(StatusCodes.Status500InternalServerError,
                                "Ocorreu um problema ao tratar a sua solicitação.");
        }
        }

        [HttpPut("{id:int}")]

        public ActionResult Put(int id, Categoria categoria)
        {

        try
        {
            if (id != categoria.CategoriaId)
            {
                return BadRequest();
            }

            _context.Entry(categoria).State = EntityState.Modified;

            return Ok(categoria);
        }
        catch (Exception)
        {

            return StatusCode(StatusCodes.Status500InternalServerError,
                                "Ocorreu um problema ao tratar a sua solicitação.");
        }
        }

        [HttpDelete("{id:int}")]
        public ActionResult Delete(int id)
        {
        try
        {
            var categoria = _context.Categorias.FirstOrDefault(c => c.CategoriaId == id);
            if (categoria is null)
            {
                return NotFound("Produto Não encontrado");
            }

            _context.Categorias.Add(categoria);
            _context.SaveChanges();
            return Ok(categoria);
        }
        catch (Exception)
        {

            return StatusCode(StatusCodes.Status500InternalServerError,
                                "Ocorreu um problema ao tratar a sua solicitação.");
        }
        }






    }
}
