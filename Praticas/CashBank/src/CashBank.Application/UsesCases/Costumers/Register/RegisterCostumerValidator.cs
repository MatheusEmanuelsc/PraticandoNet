using FluentValidation;
using CashBank.Communication.Request;
namespace CashBank.Application.UsesCases.Costumers.Register;

public class RegisterCostumerValidator: AbstractValidator<RequestRegisterCostumerJson>
{
    public RegisterCostumerValidator()
    {
        RuleFor(costumer => costumer.Name).NotEmpty().WithMessage("nome requerido");
        RuleFor(costumer => costumer.Email).NotEmpty().EmailAddress().WithMessage("email requerido");
        RuleFor(costumer => costumer.PhoneNumber).NotEmpty().WithMessage("telefone requerido");
    }
}