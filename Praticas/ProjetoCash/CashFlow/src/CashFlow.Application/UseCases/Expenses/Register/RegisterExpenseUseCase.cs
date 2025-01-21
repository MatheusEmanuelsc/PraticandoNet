
using CashFlow.Communication.Requests;
using CashFlow.Communication.Responses;
using CashFlow.Domain.Repositories;
using CashFlow.Exception.ExceptionsBase;

namespace CashFlow.Application.UseCases.Expenses.Register;

public class RegisterExpenseUseCase : IRegisterExpenseUseCase
{
    private readonly IUnitOfWork _unitOfWork;
    public RegisterExpenseUseCase(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }
    public ResponseRegisteredExpenseJson Execute(RequestRegisterExpensesJson request)
    {
        Validate(request);
        return new ResponseRegisteredExpenseJson();
    }

    private void Validate(RequestRegisterExpensesJson request)
    {
       
        
      var validator = new RegisterExpenseValidator();
      var result = validator.Validate(request);
      if (result.IsValid == false)
      {
          var errorMsg=result.Errors.Select(f => f.ErrorMessage).ToList();
          throw new ErrorOnValidationException(errorMsg);
      }

    }
}