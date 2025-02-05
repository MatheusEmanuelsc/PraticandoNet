using CashBank.Communication.Response;
using CashBank.Exception;
using CashBank.Exception.ExceptionsBase;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace CashBank.Api.Filters;

public class ExceptionFilter : IExceptionFilter
{
    public void OnException(ExceptionContext context)
    {
        if(context.Exception is CashBankException)
        {
            HandleProjectException(context);
        }
        else
        {
            ThrowUnkowError(context);
        }
    }
    private void HandleProjectException(ExceptionContext context)
    {
        if (context.Exception is CashBankException)
        {
            var exception =(ErrorOnValidationException) context.Exception;
            var errorResponse = new ResponseErrorJson(exception.Errors);
            context.HttpContext.Response.StatusCode =StatusCodes.Status400BadRequest;
            context.Result = new JsonResult(errorResponse);
        }
        else
        {
            var errorResponse = new ResponseErrorJson(context.Exception.Message);
            context.HttpContext.Response.StatusCode =StatusCodes.Status400BadRequest;
            context.Result = new JsonResult(errorResponse);
        }
    }

    private void ThrowUnkowError(ExceptionContext context)
    {
        var errorResponse = new ResponseErrorJson(ResourceErrorMessages.UNKNOWN_ERROR);

        context.HttpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
        context.Result = new ObjectResult(errorResponse);
    }
}