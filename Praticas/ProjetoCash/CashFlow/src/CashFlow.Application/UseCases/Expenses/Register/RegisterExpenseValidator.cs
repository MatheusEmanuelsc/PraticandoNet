using CashFlow.Communication.Requests;
using CashFlow.Exception;
using FluentValidation;

namespace CashFlow.Application.UseCases.Expenses.Register;

public class RegisterExpenseValidator : AbstractValidator<RequestRegisterExpensesJson>
{
    public RegisterExpenseValidator()
    {
        RuleFor(expense => expense.Title).NotEmpty().WithMessage(ResourcesErrorMessages.TITLE_IS_REQUIRED);
        RuleFor(expense => expense.Amount).GreaterThan(0).WithMessage(ResourcesErrorMessages.AMOUNT_MUST_BE_GREATER_THAN_0);
        RuleFor(expense => expense.Date).LessThanOrEqualTo(DateTime.UtcNow).WithMessage(ResourcesErrorMessages.DATE_MUST_BE_GREATER_THAN_TODAY);
        RuleFor(expense => expense.PaymentType).IsInEnum().WithMessage(ResourcesErrorMessages.PAYMENT_TYPE_IS_REQUIRED);
    }   
}