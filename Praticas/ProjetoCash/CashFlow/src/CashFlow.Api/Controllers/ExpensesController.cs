using CashFlow.Application.UseCases.Expenses.Register;
using CashFlow.Communication.Requests;
using Microsoft.AspNetCore.Mvc;

namespace CashFlow.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ExpensesController : ControllerBase
    {
        [HttpPost]
        public IActionResult Register([FromBody] RequestRegisterExpensesJson request)
        {
            
            var useCase = new RegisterExpenseUseCase();
            var response = useCase.Execute(request);
            return Created(String.Empty, response);
        }
    }
}
