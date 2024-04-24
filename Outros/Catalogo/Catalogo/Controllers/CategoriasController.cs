using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Catalogo.Context;
using Catalogo.Models;

namespace Catalogo.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class CategoriasController : ControllerBase
    {
        private readonly AppDbContext _context;

        public CategoriasController(AppDbContext context)
        {
            _context = context;
        }


        [HttpGet]
        public async Task<ActionResult<IEnumerable<Categoria>>> ListaCategorias()
        {
            return await _context.Categorias.AsNoTracking().ToListAsync();
            
        }

        [HttpGet("{id:int:min(1)}", Name = "ObterCategoria")]
        public ActionResult<Categoria> GetCategoria(int id)
        {
            var categoria = _context.Categorias.FirstOrDefault(c => c.CategoriaId == id);
            if (categoria is null)
            {
                return NotFound();
            }

            return Ok(categoria);
        }

        [HttpGet("produtos")]
        public ActionResult<IEnumerable<Categoria>> GetCategoriaProdutos()
        {
            try 
            {
                return _context.Categorias.Include(p => p.Produtos).ToList();
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,"Ocorreu um problema");
            }
        }


        [HttpPut("{id:int}")]
        public ActionResult<Categoria> AtualizaCategoria (int id, Categoria categoria)
        {
            if (id != categoria.CategoriaId) 
            {
                return BadRequest();
            }

            _context.Entry(categoria).State = EntityState.Modified;
            _context.SaveChanges();
            return Ok(categoria);
        }

        [HttpPost]
        public ActionResult<Categoria> CriaCate(Categoria categoria) 
        {
            if (categoria is null)
            {
                return BadRequest();
            }

            _context.Add(categoria);
            _context.SaveChanges();
            return  new CreatedAtRouteResult("ObterCategoria",new {id=categoria.CategoriaId},categoria);
        }

        [HttpDelete("{id:int}")]
        public ActionResult Delete(int id)
        {
            var categoria = _context.Categorias.FirstOrDefault(c => c.CategoriaId == id);
            if (categoria is null)
            {
                return NotFound();
            }

            _context.Categorias.Remove(categoria);
            _context.SaveChanges();
            return Ok(categoria);
        }
     } 
}
