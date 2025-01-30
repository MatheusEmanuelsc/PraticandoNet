# 📌 **Migrations no .NET 8 com Clean Architecture**  

## 📚 **Índice**  

1. [Introdução](#introducao)  
2. [Etapa 1: Instalação dos Pacotes](#etapa-1-instalacao-dos-pacotes)  
3. [Etapa 2: Criação da Classe de Contexto do Banco de Dados](#etapa-2-contexto-do-banco)  
4. [Etapa 3: Configuração do `appsettings.json`](#etapa-3-configuracao-do-appsettings)  
5. [Etapa 4: Configuração da Injeção de Dependência](#etapa-4-injecao-dependencia)  
6. [Etapa 5: Criando e Aplicando Migrations](#etapa-5-criando-aplicando-migrations)  
7. [Etapa 6: Removendo uma Migration](#etapa-6-removendo-migration)  
8. [Resolvendo Erros Comuns](#resolvendo-erros-comuns)  

---

### **Introdução** <a id="introducao"></a>  
Nesta seção, daremos continuidade ao desenvolvimento da aplicação, criando o banco de dados e configurando a camada de infraestrutura para utilizar o MySQL como banco de dados. Utilizaremos o **Entity Framework Core** em conjunto com o pacote **Pomelo** para conectar e gerenciar o banco MySQL.

---

### **Etapa 1: Instalação dos Pacotes** <a id="etapa-1-instalacao-dos-pacotes"></a>  

Execute os seguintes comandos no terminal para instalar os pacotes necessários:  

#### **Pacotes do Entity Framework Core e MySQL**  
```bash
# No projeto da API
cd CashBank.Api

dotnet add package Microsoft.EntityFrameworkCore.Design

# No projeto de infraestrutura
cd ../CashBank.Infrastructure

dotnet add package Microsoft.EntityFrameworkCore
dotnet add package Pomelo.EntityFrameworkCore.MySql
```

#### **Instalação da Ferramenta `dotnet-ef`**  
A ferramenta `dotnet-ef` é necessária para gerenciar migrações e o banco de dados. Instale-a com o comando:  
```bash
dotnet tool install -g dotnet-ef
```
Caso já tenha instalado, atualize-a com:  
```bash
dotnet tool update -g dotnet-ef
```

---

### **Etapa 2: Criação da Classe de Contexto do Banco de Dados** <a id="etapa-2-contexto-do-banco"></a>  

1. **Crie uma pasta chamada `DataAccess` na biblioteca de infraestrutura.**  
2. **Adicione a seguinte classe para configurar o contexto do banco de dados:**  

```csharp
using Bank.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Bank.Infrastructure.DataAccess;

/// <summary>
/// Contexto do banco de dados para a aplicação.
/// </summary>
public class BankDb : DbContext
{
    public BankDb(DbContextOptions options) : base(options)
    {
    }

    public DbSet<Account> Accounts { get; set; } = null!;
    public DbSet<Customer> Customers { get; set; } = null!;
    public DbSet<Transaction> Transactions { get; set; } = null!;
}
```

---

### **Etapa 3: Configuração do `appsettings.json`** <a id="etapa-3-configuracao-do-appsettings"></a>  

No projeto da API, abra o arquivo `appsettings.json` e adicione a string de conexão ao MySQL:  

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost;Database=BankDb;Uid=root;Pwd=b1b2b3b4;"
}
```

> Substitua **`root`** pelo usuário do banco e **`b1b2b3b4`** pela sua senha.

---

### **Etapa 4: Configuração da Injeção de Dependência** <a id="etapa-4-injecao-dependencia"></a>  

#### **Passo 1: Criação da Classe `DependencyInjectionExtension`**  

Adicione a seguinte classe na biblioteca de infraestrutura:  

```csharp
using Bank.Infrastructure.DataAccess;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Bank.Infrastructure;

/// <summary>
/// Classe para configurar as injeções de dependências da camada de infraestrutura.
/// </summary>
public static class DependencyInjectionExtension
{
    public static void AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        AddDbContext(services, configuration);
    }

    private static void AddDbContext(IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        if (string.IsNullOrEmpty(connectionString))
            throw new InvalidOperationException("Connection string 'DefaultConnection' is missing or empty.");

        var serverVersion = ServerVersion.AutoDetect(connectionString);

        services.AddDbContext<CashBankContextDb>(config => config.UseMySql(connectionString, serverVersion));
    }
}
```

#### **Passo 2: Adicione a Dependência no `Program.cs` da API**  

No arquivo `Program.cs` do projeto da API, adicione o seguinte código para registrar os serviços da camada de infraestrutura:  

```csharp
using Bank.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Adiciona a configuração da infraestrutura, incluindo o contexto do banco de dados
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

app.Run();
```

---

### **Etapa 5: Criando e Aplicando Migrations** <a id="etapa-5-criando-aplicando-migrations"></a>  

```bash
dotnet ef migrations add InitialMigration --project CashBank.Infrastructure --startup-project CashBank.Api
dotnet ef database update --project CashBank.Infrastructure --startup-project CashBank.Api
```

---

### **Etapa 6: Removendo uma Migration** <a id="etapa-6-removendo-migration"></a>  

```bash
dotnet ef migrations remove --project CashBank.Infrastructure --startup-project CashBank.Api
```

---

### **Resolvendo Erros Comuns** <a id="resolvendo-erros-comuns"></a>  

#### **"Unable to create an object of type 'BankDb'"**  
Certifique-se de que o `BankDb` está corretamente configurado no `Program.cs` e que a string de conexão está correta.

---

### **Conclusão**  
Com essas etapas, você configurou sua aplicação para usar o MySQL com Entity Framework Core no .NET 8! 🚀

