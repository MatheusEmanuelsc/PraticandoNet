using CashFlow.Domain.Repositories;
using CashFlow.Domain.Repositories.Expenses;
using CashFlow.Infrastructure.DataAcess;
using CashFlow.Infrastructure.DataAcess.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CashFlow.Infrastructure;

public  static class DependecyInjectionExtension
{
    public static void AddInfrastructure(this IServiceCollection services,IConfiguration configuration)
    {
        AddRepositories(services);
        AddDbContext(services,configuration);
    }

    private static void AddRepositories( IServiceCollection services)
    {
        services.AddScoped<IExpensesRepository,ExpensesRepository>();
        services.AddScoped<IUnitOfWork,UnitOfWork>();
    }
    private static void AddDbContext( IServiceCollection services ,IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");;
        var version = new Version(8,0, 35);
        var serverVersion = new MySqlServerVersion(version);
        services.AddDbContext<CashFlowDbContext>(config=>config.UseMySql(connectionString,serverVersion));
    }

    
}