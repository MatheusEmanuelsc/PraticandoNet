using CashFlow.Communication.Enums;
using CashFlow.Communication.Requests;
using CashFlow.Communication.Responses;

namespace CashFlow.Application.UseCases.Expenses.Register;

public class RegisterExpenseUseCase
{
    public ResponseRegisteredExpenseJson Execute(RequestRegisterExpensesJson request)
    {
        Validate(request);
        return new ResponseRegisteredExpenseJson();
    }

    private void Validate(RequestRegisterExpensesJson request)
    {
        var titleIsEmpty = string.IsNullOrWhiteSpace(request.Title);
        
        if (titleIsEmpty)
        {
            throw new ArgumentException("Title is required");
        }

        if (request.Amount <= 0)
        {
            throw new ArgumentException("Amount is required");
        }
        
       var result= DateTime.Compare(request.Date, DateTime.UtcNow) ;
       if (result > 0)
       {
           throw new ArgumentException("Date is invalid");
       }

       var paymentTypesIsValid = Enum.IsDefined(typeof(PaymentType),request.PaymentType);
       if (paymentTypesIsValid == false)
       {
           throw new ArgumentException("Payment type is invalid");
       }
    }
}