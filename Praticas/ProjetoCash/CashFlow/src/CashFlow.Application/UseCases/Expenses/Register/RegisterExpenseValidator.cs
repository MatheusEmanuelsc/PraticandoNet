using CashFlow.Communication.Requests;
using FluentValidation;

namespace CashFlow.Application.UseCases.Expenses.Register;

public class RegisterExpenseValidator : AbstractValidator<RequestRegisterExpensesJson>
{
    public RegisterExpenseValidator()
    {
        RuleFor(expense => expense.Title).NotEmpty().WithMessage("Title is required");
        RuleFor(expense => expense.Amount).GreaterThan(0).WithMessage("Amount must be greater than 0");
        RuleFor(expense => expense.Date).LessThanOrEqualTo(DateTime.UtcNow).WithMessage("Date must be greater than today");
        RuleFor(expense => expense.PaymentType).IsInEnum().WithMessage("Payment Type is required");
    }   
}