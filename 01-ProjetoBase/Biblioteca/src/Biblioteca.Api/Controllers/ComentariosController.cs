using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Biblioteca.Api.Controllers
{
    [Route("api/Livros/{LivrosId:int}/[controller]")]
    [ApiController]
    public class ComentariosController : ControllerBase
    {
    }
}
