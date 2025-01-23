using AutoMapper;
using CashFlow.Communication.Requests;
using CashFlow.Domain.Entities;

namespace CashFlow.Application.AutoMapper;

public class AutoMapping : Profile
{

    public AutoMapping()
    {
        RequestToEntity();
        EntityToRequest();
    }
    private void RequestToEntity()
    {
        CreateMap<RequestRegisterExpensesJson, Expense>();
    }
    
    private void EntityToRequest()
    {
        CreateMap<Expense,RequestRegisterExpensesJson>();
    }
}

