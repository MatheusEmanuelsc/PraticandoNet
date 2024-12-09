using CashFlow.Application.UseCases.Expenses.Register;
using CashFlow.Communication.Requests;
using CashFlow.Communication.Responses;
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
            try
            {
                var useCase = new RegisterExpenseUseCase();
                var response = useCase.Execute(request);
                return Created(String.Empty, response);
            }
            catch (ArgumentException e)
            {
                var errorResponse = new ResponseErrorJson(e.Message);
                
                return BadRequest(errorResponse);
            }
            catch
            {
                var errorResponse = new ResponseErrorJson("An unexpected error occurred.");
                
                return StatusCode(StatusCodes.Status500InternalServerError , errorResponse);
            }
        }
    }
}
