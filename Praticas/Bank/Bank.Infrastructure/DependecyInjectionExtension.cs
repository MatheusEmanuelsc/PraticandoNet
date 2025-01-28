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
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        if (string.IsNullOrEmpty(connectionString))
            throw new InvalidOperationException("Connection string 'DefaultConnection' is missing or empty.");

        // Detecta automaticamente a versão do servidor MySQL
        services.AddDbContext<BankContextDb>(options =>
            options.UseMySql(connectionString, MySqlServerVersion.AutoDetect(connectionString)));
    }
}