using CashBank.Application.AutoMapper;
using Microsoft.Extensions.DependencyInjection;

namespace CashBank.Application;

public static class DependecyInjectExtension
{
    public static void AddApplication(this IServiceCollection services)
    {
        AddAutoMapper(services);
    }
    
    private static void AddAutoMapper(IServiceCollection services)
    {
        services.AddAutoMapper(typeof(AutoMapping));
    }
}
   