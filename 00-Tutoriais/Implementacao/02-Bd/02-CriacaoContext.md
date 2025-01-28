### Índice  

1. [Introdução](#introducao)  
2. [Etapa 1: Instalação dos Pacotes](#etapa-1-instalacao-dos-pacotes)  
3. [Etapa 2: Criação da Classe de Contexto do Banco de Dados](#etapa-2-contexto-do-banco)  
4. [Etapa 3: Configuração do `appsettings.json`](#etapa-3-configuracao-do-appsettings)  
5. [Etapa 4: Configuração da Injeção de Dependência](#etapa-4-injecao-dependencia)  

---

### **Introdução** <a id="introducao"></a>  

Nesta seção, daremos continuidade ao desenvolvimento da aplicação, criando o banco de dados e configurando a camada de infraestrutura para utilizar o MySQL como banco de dados. Utilizaremos o **Entity Framework Core** em conjunto com o pacote **Pomelo** para conectar e gerenciar o banco MySQL.

---

### **Etapa 1: Instalação dos Pacotes** <a id="etapa-1-instalacao-dos-pacotes"></a>  

Execute os seguintes comandos no terminal para instalar os pacotes necessários:  

#### **Pacotes do Entity Framework Core e MySQL**  
```bash

dotnet add package Microsoft.EntityFrameworkCore.Design
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

namespace Bank.Infrastructure.DataAcess;

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

Para centralizar a configuração do banco de dados, criaremos uma classe para configurar a injeção de dependência.

#### **Passo 1: Criação da Classe `DependencyInjectionExtension`**  

Adicione a seguinte classe na biblioteca de infraestrutura:  

```csharp
using Bank.Infrastructure.DataAcess;
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

        // Configura o DbContext utilizando o MySQL
        services.AddDbContext<BankDb>(options =>
            options.UseMySql(connectionString, MySqlServerVersion.AutoDetect(connectionString)));
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

### **Conclusão**  

Com essas etapas, você configurou:  
1. O **Entity Framework Core** para se comunicar com um banco de dados MySQL.  
2. O contexto do banco de dados na pasta `DataAccess`.  
3. A string de conexão no arquivo `appsettings.json`.  
4. A injeção de dependência do contexto do banco de dados.  

Agora, você está pronto para criar as migrações e gerar as tabelas no banco de dados!