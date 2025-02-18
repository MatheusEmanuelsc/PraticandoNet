using CashFlow.Application.AutoMapper;
using CashFlow.Application.UseCases.Expenses.Register;
using Microsoft.Extensions.DependencyInjection;

namespace CashFlow.Application;

public static class DependecyInjectionExtension
{
    public static void AddApplication(this IServiceCollection services)
    {
        
        AddAutoMapper(services);
        AddUseCases(services);
    }
    
    private static void AddAutoMapper( IServiceCollection services)
    {
        services.AddAutoMapper(typeof(AutoMapping));
    }
    
    private static void AddUseCases( IServiceCollection services)
    {
        services.AddScoped<IRegisterExpenseUseCase,RegisterExpenseUseCase>();
    }
}