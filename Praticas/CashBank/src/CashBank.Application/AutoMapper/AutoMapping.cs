using AutoMapper;
using CashBank.Communication.Request;
using CashBank.Domain.Entities;

namespace CashBank.Application.AutoMapper;

public class AutoMapping :Profile
{
    public AutoMapping()
    {
        RequestToEntity();
        EntityToRequest();
    }
    
    private void RequestToEntity()
    {
        CreateMap<RequestRegisterCostumerJson, Customer>();
    }
    
    private void EntityToRequest()
    {
        CreateMap<Customer,RequestRegisterCostumerJson>();
    }
}

