

# Injeção de Dependências no ASP.NET Core

## Índice
1. [O que é Injeção de Dependências?](#o-que-é-injeção-de-dependências)
2. [Injeção de Dependências no ASP.NET Core](#injeção-de-dependências-no-aspnet-core)
   - [Contêiner de DI Nativo](#contêiner-de-di-nativo)
   - [Escopos de Vida](#escopos-de-vida)
3. [Configurando a Injeção de Dependências](#configurando-a-injeção-de-dependências)
   - [Métodos Básicos](#métodos-básicos)
   - [Exemplo Simples](#exemplo-simples)
4. [Criando uma Classe de Extensão para Injeções](#criando-uma-classe-de-extensão-para-injeções)
   - [Por que usar uma classe de extensão?](#por-que-usar-uma-classe-de-extensão)
   - [Exemplo com Classe de Extensão](#exemplo-com-classe-de-extensão)
5. [Boas Práticas](#boas-práticas)
6. [Conclusão](#conclusão)

---

## O que é Injeção de Dependências?

Injeção de Dependências (DI, do inglês *Dependency Injection*) é um padrão de design que permite a separação de responsabilidades e a inversão de controle (IoC). Em vez de uma classe criar suas próprias dependências, elas são fornecidas (ou "injetadas") por um contêiner ou framework externo. Isso melhora a modularidade, testabilidade e manutenção do código.

---

## Injeção de Dependências no ASP.NET Core

O ASP.NET Core possui um sistema de injeção de dependências embutido, baseado no princípio de IoC. Ele elimina a necessidade de frameworks externos como Ninject ou Autofac (embora possam ser usados, se desejado).

### Contêiner de DI Nativo
O contêiner de DI no ASP.NET Core é configurado no arquivo `Program.cs` (ou `Startup.cs` em versões mais antigas). Ele usa o `IServiceCollection` para registrar serviços que podem ser injetados em controladores, serviços ou outros componentes.

### Escopos de Vida
O ASP.NET Core suporta três escopos principais para serviços:
- **Transient**: Uma nova instância é criada a cada solicitação.
- **Scoped**: Uma instância é criada por solicitação HTTP.
- **Singleton**: Uma única instância é criada e compartilhada por toda a aplicação.

---

## Configurando a Injeção de Dependências

### Métodos Básicos
Os métodos mais comuns para registrar serviços no `IServiceCollection` são:
- `AddTransient<TService>`: Registro como Transient.
- `AddScoped<TService>`: Registro como Scoped.
- `AddSingleton<TService>`: Registro como Singleton.

### Exemplo Simples
Aqui está um exemplo básico de configuração no `Program.cs`:

```csharp
var builder = WebApplication.CreateBuilder(args);

// Registro de serviços
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddTransient<ILoggerService, LoggerService>();
builder.Services.AddSingleton<IConfigService, ConfigService>();

builder.Services.AddControllers();
var app = builder.Build();

app.UseRouting();
app.MapControllers();
app.Run();

public interface IUserService { void DoSomething(); }
public class UserService : IUserService { public void DoSomething() { } }

public interface ILoggerService { void Log(string message); }
public class LoggerService : ILoggerService { public void Log(string message) { } }

public interface IConfigService { string GetConfig(); }
public class ConfigService : IConfigService { public string GetConfig() => "Config"; }
```

Nesse exemplo, `IUserService` é Scoped, `ILoggerService` é Transient e `IConfigService` é Singleton.

---

## Criando uma Classe de Extensão para Injeções

### Por que usar uma classe de extensão?
À medida que o projeto cresce, o `Program.cs` pode ficar desorganizado com muitos registros de serviços. Uma classe de extensão ajuda a manter o código limpo e modular, agrupando as injeções relacionadas.

### Exemplo com Classe de Extensão
Vamos criar uma classe de extensão para organizar as injeções.

1. **Crie a classe de extensão**:

```csharp
namespace MeuProjeto.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IUserService, UserService>();
        services.AddTransient<ILoggerService, LoggerService>();
        services.AddSingleton<IConfigService, ConfigService>();
        
        return services; // Retorna o IServiceCollection para encadeamento
    }
}
```

2. **Atualize o `Program.cs`**:

```csharp
using MeuProjeto.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Uso da classe de extensão
builder.Services.AddApplicationServices();

builder.Services.AddControllers();
var app = builder.Build();

app.UseRouting();
app.MapControllers();
app.Run();
```

3. **Exemplo de uso em um controlador**:

```csharp
public class UserController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly ILoggerService _loggerService;
    private readonly IConfigService _configService;

    public UserController(IUserService userService, ILoggerService loggerService, IConfigService configService)
    {
        _userService = userService;
        _loggerService = loggerService;
        _configService = configService;
    }

    [HttpGet]
    public IActionResult Get()
    {
        _userService.DoSomething();
        _loggerService.Log("Ação realizada");
        var config = _configService.GetConfig();
        return Ok($"Config: {config}");
    }
}
```

Nesse exemplo, as dependências são injetadas automaticamente no construtor do controlador, graças ao contêiner de DI configurado.

---

## Boas Práticas

1. **Escolha o escopo correto**: Use `Scoped` para serviços relacionados a uma requisição HTTP, `Transient` para operações leves e `Singleton` para estados compartilhados ou configurações.
2. **Evite sobrecarga no `Program.cs`**: Utilize classes de extensão ou módulos para organizar os registros.
3. **Prefira interfaces**: Registre serviços usando interfaces para facilitar a substituição de implementações e testes.
4. **Valide as dependências**: Certifique-se de que todas as dependências necessárias estão registradas para evitar erros em tempo de execução.

---

## Conclusão

A injeção de dependências no ASP.NET Core é uma ferramenta poderosa para criar aplicações modulares e testáveis. Usar uma classe de extensão, como mostrado no exemplo, ajuda a manter o código organizado e escalável. Com os escopos de vida adequados e boas práticas, você pode aproveitar ao máximo esse recurso nativo do framework.

