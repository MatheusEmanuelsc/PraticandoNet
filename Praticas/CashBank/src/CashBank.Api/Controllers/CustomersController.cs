using CashBank.Application.UsesCases.Costumers.Register;
using CashBank.Communication.Request;
using CashBank.Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CashBank.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CustomersController : ControllerBase
    {

        public IActionResult Register([FromBody] RequestRegisterCostumerJson register)
        {
            var useCase = new RegisterCostumerUseCase();
            var response = useCase.Execute(register);
            return Created(string.Empty, response);
        }
    }
}
