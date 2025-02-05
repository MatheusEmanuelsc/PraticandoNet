using FluentValidation;
using CashBank.Communication.Request;
using CashBank.Exception;

namespace CashBank.Application.UsesCases.Costumers.Register;

public class RegisterCostumerValidator: AbstractValidator<RequestRegisterCostumerJson>
{
    public RegisterCostumerValidator()
    {
        RuleFor(costumer => costumer.Name).NotEmpty().WithMessage(ResourceErrorMessages.NAME_REQUIRED);
        RuleFor(costumer => costumer.Email).NotEmpty().EmailAddress().WithMessage(ResourceErrorMessages.EMAIL_REQUIRED);
        RuleFor(costumer => costumer.PhoneNumber).NotEmpty().WithMessage(ResourceErrorMessages.PHONE_NUMBER_REQUIRED);
    }
}