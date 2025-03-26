using Biblioteca.Api.Context;
using Biblioteca.Api.Entidades;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Biblioteca.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AutoresController : ControllerBase
    {
        private readonly AppDbContext _context;
        public AutoresController( AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var autores = await _context.Autores.ToListAsync();
            return Ok(autores);
        }
        
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetId(int id)
        {
            var autor = await _context.Autores.FirstOrDefaultAsync(a => a.Id == id);
            if (autor == null)
            {
                return NotFound();
            }
                
            return Ok(autor);
        }

        [HttpPost]
        public async Task<IActionResult> Post(Autor autor)
        {
            _context.Add(autor);
            await _context.SaveChangesAsync();
            return Ok(autor);
        }
        
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Put(Autor novoAutor, int id)
        {
            var autor = await _context.Autores.FirstOrDefaultAsync(a => a.Id == id);
            if (autor == null || autor.Id != id)
            {
                return NotFound();
            }
            
            _context.Update(novoAutor);
            await _context.SaveChangesAsync();
            return Ok(novoAutor);
        }
        
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete( int id)
        {
            var autor = await _context.Autores.FirstOrDefaultAsync(a => a.Id == id);
            if (autor == null )
            {
                return NotFound();
            }
            
            _context.Remove(autor);
            await _context.SaveChangesAsync();
            return Ok(autor);
        }
    }
}
