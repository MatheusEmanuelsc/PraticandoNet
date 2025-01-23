
using CashFlow.Communication.Requests;
using CashFlow.Communication.Responses;
using CashFlow.Domain.Entities;
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
    public async Task<ResponseRegisteredExpenseJson> Execute(RequestRegisterExpensesJson request)
    {
        Validate(request);
        var entity = new Expense
        {
            Amount = request.Amount,
            Description = request.Description,
            Date = request.Date,
            Title = request.Title,
            PaymentType =(Domain.Enums.PaymentType) request.PaymentType,
        };
        
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