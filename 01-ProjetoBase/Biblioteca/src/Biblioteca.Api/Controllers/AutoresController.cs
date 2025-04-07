using AutoMapper;
using Biblioteca.Api.Context;
using Biblioteca.Api.Dtos;
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
        private readonly IMapper _mapper;
        public AutoresController( AppDbContext context,IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var autores = await _context.Autores.ToListAsync();
            var autoresDto = _mapper.Map<AutorReadDto>(autores);
            return Ok(autoresDto);
        }
        
        [HttpGet("{id:int}",Name = "GetAutor")]
        public async Task<IActionResult> GetId(int id)
        {
            var autor = await _context.Autores.FirstOrDefaultAsync(a => a.Id == id);
            if (autor == null)
            {
                return NotFound();
            }
            var autoresDto = _mapper.Map<AutorReadDto>(autor);
            return Ok(autoresDto);
        }

        [HttpPost]
        public async Task<IActionResult> Post(AutorCreateDto autorCreateDto)
        {
            var autor =_mapper.Map<Autor>(autorCreateDto);
            _context.Add(autor);
            await _context.SaveChangesAsync();
            var autorDto = _mapper.Map<AutorReadDto>(autor);
            return CreatedAtRoute("GetAutor", new { id = autorDto.Id }, autorDto);
        }
        
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Put(AutorCreateDto autorCreateDto, int id)
        {
            var autor = _mapper.Map<Autor>(autorCreateDto);
            autor.Id = id;
            _context.Update(autor);
            await _context.SaveChangesAsync();
            return NoContent();
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
