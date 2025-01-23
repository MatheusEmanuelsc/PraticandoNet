using CashFlow.Communication.Requests;
using CashFlow.Communication.Responses;

namespace CashFlow.Application.UseCases.Expenses.Register;

public interface IRegisterExpenseUseCase
{
    public Task< ResponseRegisteredExpenseJson> Execute(RequestRegisterExpensesJson request);
}