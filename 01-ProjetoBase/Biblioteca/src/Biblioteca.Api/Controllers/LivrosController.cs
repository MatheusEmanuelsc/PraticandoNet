using AutoMapper;
using Biblioteca.Api.Context;
using Biblioteca.Api.Dtos;
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
        private readonly IMapper _mapper;
        public LivrosController( AppDbContext context , IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }
        
        
        [HttpGet]
        public async Task<IEnumerable<LivroReadDto>> Get()
        {
            var livros =await _context.Livros.ToListAsync();
            var livrosDto = _mapper.Map<IEnumerable<LivroReadDto>>(livros);
            return livrosDto;
        }

        [HttpGet("{id:int}",Name = "GetLivro")]
        public async Task<ActionResult<LivroComAutorDto>> Get(int id)
        {
            var libro = await _context.Livros
                .Include(x => x.Autor)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (libro is null)
            {
                return NotFound();
            }
            
           var livroDto = _mapper.Map<LivroComAutorDto>(libro);
            return livroDto;
        }

        [HttpPost]
        public async Task<ActionResult> Post(LivroCreateDto livroCreateDto)
        {
            var livro = _mapper.Map<Livro>(livroCreateDto);
            var existeAutor = await _context.Autores.AnyAsync(x => x.Id == livro.AutorId);

            if (!existeAutor)
            {
                return BadRequest($"El autor de id {livro.AutorId} no existe");
            }

            _context.Add(livro);
            await _context.SaveChangesAsync();
            var livroReadDto = _mapper.Map<LivroComAutorDto>(livro);
            return CreatedAtRoute("GetLivro", new { id = livro.Id }, livroReadDto);
            
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult> Put(int id, LivroCreateDto livroCreateDto)
        {
            var livro = _mapper.Map<Livro>(livroCreateDto);
            livro.Id = id;

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
