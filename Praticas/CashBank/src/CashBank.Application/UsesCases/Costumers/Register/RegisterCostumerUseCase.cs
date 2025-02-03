using CashBank.Communication.Request;
using CashBank.Communication.Response;

namespace CashBank.Application.UsesCases.Costumers.Register;

public class RegisterCostumerUseCase
{
    public ResponseCostumerJson Execute( RequestRegisterCostumerJson registe)
    {
        return new ResponseCostumerJson();
    }

    private void Validate(RequestRegisterCostumerJson request)
    {
        var validator = new RegisterCostumerValidator();
        var result = validator.Validate(request);
        if (!result.IsValid)
        {
            var validationErrors = result.Errors.Select(x => x.ErrorMessage).ToList();
            throw new ArgumentException();
        }
        
    }
}