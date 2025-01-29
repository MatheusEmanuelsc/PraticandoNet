using Bank.Infrastructure.DataAcess;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Bank.Infrastructure;

public static class DependecyInjectionExtension
{
    public static void  AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        AddDbContext(services, configuration);
    }
    private static void AddDbContext( IServiceCollection services ,IConfiguration configuration)
    {
        

        var mysqlConnection = configuration.GetConnectionString("DefaultConnection");
        services.AddDbContext<BankContextDb>(options =>
        {
            options.UseMySql(mysqlConnection, ServerVersion.AutoDetect(mysqlConnection));
        });
        

    }
}