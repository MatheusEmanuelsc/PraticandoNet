using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using _04_Database.Domain.Model;
using _04_Database.Domain;

namespace _04_Database.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AtendimentoController : ControllerBase
    {
        private AppDBContext _appDBContext;

        public AtendimentoController(AppDBContext appDBContext)
        {
            _appDBContext = appDBContext;
        }

        [HttpGet]
        public IActionResult TodosAtendimento(){
            var atendimento = _appDBContext.Atendimentos.ToList();
            return Ok(atendimento);
        }

        [HttpPost]
        public IActionResult Registro(Atendimento atendimento){
             _appDBContext.Atendimentos.Add(atendimento);
            _appDBContext.SaveChanges();
            return Ok("Criado");
        }
    }
}