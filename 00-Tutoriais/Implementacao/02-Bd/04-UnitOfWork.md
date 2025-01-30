# Implementando Unit of Work no .NET 8  

## Índice  

1. [Introdução](#introdução)  
2. [Criando a Interface IUnitOfWork](#criando-a-interface-iunitofwork)  
3. [Implementando a Classe UnitOfWork](#implementando-a-classe-unitofwork)  
4. [Registrando a Dependência do Unit of Work](#registrando-a-dependência-do-unit-of-work)  
5. [Próximos Passos](#próximos-passos)  

---

## Introdução  

O padrão **Unit of Work** é utilizado para gerenciar transações no banco de dados, garantindo que todas as operações sejam concluídas corretamente antes de confirmar a persistência das mudanças. Isso ajuda a manter a consistência e a integridade dos dados em aplicações que utilizam o **Entity Framework Core**.  

Neste tutorial, implementaremos o **Unit of Work** em um projeto **.NET 8**, organizando o código de forma modular para facilitar a manutenção e reutilização.  

---

## Criando a Interface `IUnitOfWork`  

Começamos criando a interface `IUnitOfWork` dentro da pasta `Repositories` no projeto **Domain**. Ela conterá apenas um método para confirmar as transações:  

📌 **Crie o arquivo `IUnitOfWork.cs` em `CashBank.Domain.Repositories`:**  

```csharp
namespace CashBank.Domain.Repositories;

public interface IUnitOfWork
{
    Task Commit();
}
```

Essa interface define um contrato para garantir que qualquer implementação execute a operação de **commit** no banco de dados.  

---

## Implementando a Classe `UnitOfWork`  

Agora, criamos a implementação dessa interface dentro da camada **Infrastructure**, mais especificamente na pasta **DataAcess**.  

📌 **Crie o arquivo `UnitOfWork.cs` em `CashBank.Infrastructure.DataAcess`:**  

```csharp
using CashBank.Domain.Repositories;

namespace CashBank.Infrastructure.DataAcess;

public class UnitOfWork : IUnitOfWork
{
    private readonly CashBankContextDb _context;

    public UnitOfWork(CashBankContextDb context)
    {
        _context = context;
    }

    public async Task Commit()
    {
        await _context.SaveChangesAsync();
    }
}
```

### Explicação  

- **`CashBankContextDb _context;`** → Armazena a instância do **DbContext** da aplicação.  
- **Construtor `UnitOfWork(CashBankContextDb context)`** → Injeta o contexto no **UnitOfWork**.  
- **Método `Commit()`** → Salva as mudanças pendentes no banco de dados chamando `_context.SaveChangesAsync();`.  

---

## Registrando a Dependência do Unit of Work  

Para que o `UnitOfWork` possa ser utilizado por outros serviços, precisamos registrá-lo na **injeção de dependência (DI)**.  

### 1️⃣ Adicionando a Configuração na `DependencyInjectionExtension`  

📌 **Edite o arquivo `DependencyInjectionExtension.cs` em `CashBank.Infrastructure`:**  

```csharp
private static void AddRepositories(IServiceCollection services)
{
    services.AddScoped<IUnitOfWork, UnitOfWork>();
}
```

### 2️⃣ Chamando a Configuração no Método `AddInfrastructure`  

Certifique-se de que o método `AddInfrastructure` chame a função `AddRepositories`:  

```csharp
public static void AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
{
    AddDbContext(services, configuration);
    AddRepositories(services);
}
```

Agora, sempre que um serviço ou repositório precisar de uma instância de `IUnitOfWork`, o .NET injetará automaticamente um `UnitOfWork` funcional.  

---

## Próximos Passos  

Agora que implementamos o **Unit of Work**, o próximo passo é utilizá-lo nas regras de negócio. Devemos injetá-lo nos serviços responsáveis pelas operações com o banco de dados, garantindo que todas as ações sejam realizadas dentro de uma única transação.  

**Nos próximos passos, faremos:**  
✔ Implementação de repositórios que utilizam o **Unit of Work**.  
✔ Uso do `Commit()` para garantir que múltiplas operações sejam concluídas corretamente.  
✔ Aplicação do padrão em serviços da camada de **Application**.  

Com isso, garantimos que todas as operações em nossa aplicação sejam transacionais e seguras! 🚀