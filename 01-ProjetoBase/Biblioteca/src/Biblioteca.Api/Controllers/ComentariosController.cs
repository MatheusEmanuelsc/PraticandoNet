using AutoMapper;
using Biblioteca.Api.Context;
using Biblioteca.Api.Dtos;
using Biblioteca.Api.Entidades;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Biblioteca.Api.Controllers
{
    [Route("api/Livros/{livrosId:int}/[controller]")]
    [ApiController]
    public class ComentariosController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;
        public ComentariosController( AppDbContext context, IMapper mapper)
        {
             _context = context;
             _mapper = mapper;
        }

        [HttpGet]
        public async Task<IActionResult> Get(int livrosId)
        {
            var livro = await _context.Livros.AnyAsync(x => x.Id == livrosId);
            if (!livro)
            {
                return NotFound();
            }
            var comentarios = await _context.Comentarios
                .Where(x=> x.LivroId == livrosId)
                .OrderByDescending(x=> x.FechaPublicacion)
                .ToListAsync();
            var comentariosDto = _mapper.Map<ComentarioReadDto>(comentarios);
            return Ok(comentariosDto);
        }

        [HttpGet("{id}", Name = "ObtenerComentario")]
        public async Task<ActionResult<ComentarioReadDto>> Get(Guid id)
        {
            var comentario = await _context.Comentarios.FirstOrDefaultAsync(x => x.Id == id);

            if (comentario is null)
            {
                return NotFound();
            }

            return _mapper.Map<ComentarioReadDto>(comentario);
        }

        [HttpPost]
        public async Task<IActionResult> Post(ComentarioCreateDto comentarioCreateDto , int livrosId)
        {
            var livro = await _context.Livros.AnyAsync(x => x.Id == livrosId);
            if (!livro)
            {
                return NotFound();
            }
            var comentario = _mapper.Map<Comentario>(comentarioCreateDto);
            comentario.LivroId = livrosId;
            comentario.FechaPublicacion = DateTime.UtcNow;
            _context.Comentarios.Add(comentario);
            await _context.SaveChangesAsync();
            var comentarioReadDto = _mapper.Map<ComentarioReadDto>(comentario);
            return CreatedAtRoute( "ObtenerComentario",new {id = comentario.Id,livrosId}, comentarioReadDto );
        } 
        
        [HttpPatch("{id}")]
        public async Task<ActionResult> Patch(Guid id, int libroId, JsonPatchDocument<ComentarioPatchDto> patchDoc)
        {
            if (patchDoc is null)
            {
                return BadRequest();
            }

            var existeLibro = await _context.Livros.AnyAsync(x => x.Id == libroId);

            if (!existeLibro)
            {
                return NotFound();
            }

            var comentarioDB = await _context.Comentarios.FirstOrDefaultAsync(x => x.Id == id);

            if (comentarioDB is null)
            {
                return NotFound();
            }

            var comentarioPatchDTO = _mapper.Map<ComentarioPatchDto>(comentarioDB);

            patchDoc.ApplyTo(comentarioPatchDTO, ModelState);

            var esValido = TryValidateModel(comentarioPatchDTO);

            if (!esValido)
            {
                return ValidationProblem();
            }

            _mapper.Map(comentarioPatchDTO, comentarioDB);

            await _context.SaveChangesAsync();

            return NoContent();
        }
        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(Guid id, int livroId)
        {
            var existeLibro = await _context.Livros.AnyAsync(x => x.Id == livroId);

            if (!existeLibro)
            {
                return NotFound();
            }

            var registrosBorrados = await _context.Comentarios.Where(x => x.Id == id).ExecuteDeleteAsync();

            if (registrosBorrados == 0)
            {
                return NotFound();
            }

            return NoContent();
        }

    }
}
