using Biblioteca.Api.Context;
using Biblioteca.Api.Entidades;
using Microsoft.AspNetCore.Http;
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
        public async Task<ActionResult<IEnumerable<Livro>>> GetLivros()
    }
}
