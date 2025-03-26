using Biblioteca.Api.Entidades;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Biblioteca.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AutoresController : ControllerBase
    {

        [HttpGet]
        public IActionResult Get()
        {
            return Ok(
                new List<Autor>
                {
                    new Autor {Nome = "Lucas"},
                }
                );
        }
    }
}
