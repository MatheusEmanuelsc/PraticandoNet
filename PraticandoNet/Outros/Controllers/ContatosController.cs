using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Outros.Context;
using Outros.models;

namespace Outros.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ContatosController : ControllerBase
    {
        private readonly AgendaContext _context;
        public ContatosController(AgendaContext context)
        {
            _context = context;
        }

        [HttpPost]
        public IActionResult Create(Contato contato){
            _context.Add(contato);
            _context.SaveChanges();
            return CreatedAtAction(nameof(ObeterPorId),new {id = contato.ContatoId},contato);
        }

        [HttpGet("{id}")]
        public IActionResult ObeterPorId(int id){
            var contato = _context.Contatos.Find(id);

            if (contato is null)
            {
                return NotFound();
            }
            return Ok(contato);
        }

        [HttpGet("buscarPorNome")]
        public IActionResult ObeterPorNome(string nome){
            var contato = _context.Contatos.Where(x=> x.Nome.Contains(nome));

            
            return Ok(contato);
        }

        [HttpPut("{id}")]
        public IActionResult Atualizar(int id,Contato contato){
            
            var contatoBanco = _context.Contatos.Find(id);

            if (contato is null)
            {
                return NotFound();
            }

            contatoBanco.Nome = contato.Nome;
            contatoBanco.Telefone = contato.Telefone;
            contatoBanco.Ativo = contato.Ativo;

            _context.Contatos.Update(contatoBanco);
            _context.SaveChanges();
            return Ok(contatoBanco);
        }

        
        [HttpDelete("{id}")]
        public IActionResult Deletar(int id){
            var contato = _context.Contatos.Find(id);

            if (contato is null)
            {
                return NotFound();
            }
            _context.Contatos.Remove(contato);
            _context.SaveChanges();
            return NoContent();
        }
    }
}