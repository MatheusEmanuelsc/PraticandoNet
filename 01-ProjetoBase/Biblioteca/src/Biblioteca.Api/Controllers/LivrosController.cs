using Biblioteca.Api.Context;
using Biblioteca.Api.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;

namespace Biblioteca.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LivrosController : ControllerBase
    {
        private readonly AppDbContext _context;
        public LivrosController( AppDbContext context)
        {
            _context = context;
        }
        
        
        [HttpGet]
        public async Task<IEnumerable<Livro>> Get()
        {
            return await _context.Livros.ToListAsync();
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<Livro>> Get(int id)
        {
            var libro = await _context.Livros
                .Include(x => x.Autor)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (libro is null)
            {
                return NotFound();
            }

            return libro;
        }

        [HttpPost]
        public async Task<ActionResult> Post(Livro livro)
        {
            var existeAutor = await _context.Autores.AnyAsync(x => x.Id == livro.AutorId);

            if (!existeAutor)
            {
                return BadRequest($"El autor de id {livro.AutorId} no existe");
            }

            _context.Add(livro);
            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult> Put(int id, Livro livro)
        {
            if (id != livro.Id)
            {
                return BadRequest("Los ids deben de coincidir");
            }

            var existeAutor = await _context.Autores.AnyAsync(x => x.Id == livro.AutorId);

            if (!existeAutor)
            {
                return BadRequest($"El autor de id {livro.AutorId} no existe");
            }

            _context.Update(livro);
            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpDelete("{id:int}")]
        public async Task<ActionResult> Delete(int id)
        {
            var registrosBorrados = await _context.Livros.Where(x => x.Id == id).ExecuteDeleteAsync();

            if (registrosBorrados == 0) 
            {
                return NotFound();
            }

            return Ok();
        }
    }
}
