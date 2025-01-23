
using AutoMapper;
using CashFlow.Communication.Requests;
using CashFlow.Communication.Responses;
using CashFlow.Domain.Entities;
using CashFlow.Domain.Repositories;
using CashFlow.Exception.ExceptionsBase;

namespace CashFlow.Application.UseCases.Expenses.Register;

public class RegisterExpenseUseCase : IRegisterExpenseUseCase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    public RegisterExpenseUseCase(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }
    public async Task<ResponseRegisteredExpenseJson> Execute(RequestRegisterExpensesJson request)
    {
        Validate(request);
        var entity =_mapper.Map<Expense>(request);
        
        
        
        return _mapper.Map<ResponseRegisteredExpenseJson>(entity);
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