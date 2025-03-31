
---

# Loggers no ASP.NET Core

## Índice
1. [O que são Loggers?](#o-que-são-loggers)
2. [Loggers no ASP.NET Core](#loggers-no-aspnet-core)
   - [Framework de Logging Nativo](#framework-de-logging-nativo)
   - [Níveis de Log](#níveis-de-log)
3. [Configurando Loggers](#configurando-loggers)
   - [Métodos Básicos](#métodos-básicos)
   - [Exemplo Simples](#exemplo-simples)
4. [Criando uma Classe de Extensão para Loggers](#criando-uma-classe-de-extensão-para-loggers)
   - [Por que usar uma classe de extensão?](#por-que-usar-uma-classe-de-extensão)
   - [Exemplo com Classe de Extensão](#exemplo-com-classe-de-extensão)
5. [Boas Práticas](#boas-práticas)
6. [Conclusão](#conclusão)

---

## O que são Loggers?

Loggers são ferramentas usadas para registrar eventos, erros, informações ou atividades em uma aplicação. Eles ajudam no monitoramento, depuração e auditoria, permitindo que os desenvolvedores acompanhem o comportamento do sistema em tempo real ou após a execução.

---

## Loggers no ASP.NET Core

O ASP.NET Core oferece um framework de logging integrado que é flexível, extensível e suporta múltiplos provedores (como console, arquivos, ou serviços externos como Serilog e NLog).

### Framework de Logging Nativo
O logging no ASP.NET Core é baseado na interface `ILogger` e no serviço `ILogger<T>`, que é injetado automaticamente via injeção de dependências. Ele é configurado no `Program.cs` e pode ser personalizado com provedores e filtros.

### Níveis de Log
Os níveis de log disponíveis são:
- **Trace**: Informações detalhadas, geralmente para depuração profunda.
- **Debug**: Informações úteis durante o desenvolvimento.
- **Information**: Eventos gerais da aplicação.
- **Warning**: Indica algo potencialmente problemático.
- **Error**: Erros que afetam a execução.
- **Critical**: Falhas graves que exigem atenção imediata.

---

## Configurando Loggers

### Métodos Básicos
O logging é configurado automaticamente pelo ASP.NET Core, mas você pode personalizá-lo:
- `AddConsole()`: Adiciona logs no console.
- `AddDebug()`: Adiciona logs visíveis em ferramentas de depuração.
- `AddFile()` (via pacotes como `Serilog`): Adiciona logs em arquivos.

Os níveis e filtros podem ser ajustados no `appsettings.json` ou diretamente no código.

### Exemplo Simples
Aqui está um exemplo básico configurando logs no `Program.cs`:

```csharp
var builder = WebApplication.CreateBuilder(args);

// Configuração básica de logging
builder.Logging.ClearProviders(); // Remove provedores padrão
builder.Logging.AddConsole();     // Adiciona logs no console
builder.Logging.AddDebug();       // Adiciona logs para depuração

builder.Services.AddControllers();
var app = builder.Build();

app.UseRouting();
app.MapControllers();
app.Run();

// Exemplo de uso em um controlador
public class TestController : ControllerBase
{
    private readonly ILogger<TestController> _logger;

    public TestController(ILogger<TestController> logger)
    {
        _logger = logger;
    }

    [HttpGet]
    public IActionResult Get()
    {
        _logger.LogInformation("Requisição GET recebida");
        return Ok("Sucesso");
    }
}
```

Nesse exemplo, o logger registra uma mensagem no console e no debug quando a rota é acessada.

---

## Criando uma Classe de Extensão para Loggers

### Por que usar uma classe de extensão?
Configurações de logging podem se tornar complexas com múltiplos provedores e regras. Uma classe de extensão mantém o `Program.cs` limpo e organiza a lógica de logging em um único lugar.

### Exemplo com Classe de Extensão
Vamos criar uma classe de extensão para configurar o logging.

1. **Crie a classe de extensão**:

```csharp
namespace MeuProjeto.Extensions;

public static class LoggingExtensions
{
    public static ILoggingBuilder AddCustomLogging(this ILoggingBuilder logging)
    {
        logging.ClearProviders();           // Remove provedores padrão
        logging.AddConsole();               // Logs no console
        logging.AddDebug();                 // Logs no debug
        logging.SetMinimumLevel(LogLevel.Information); // Nível mínimo
        
        // Exemplo de filtro personalizado
        logging.AddFilter("Microsoft", LogLevel.Warning); // Apenas warnings do Microsoft
        logging.AddFilter("System", LogLevel.Error);      // Apenas erros do System
        
        return logging; // Retorna o ILoggingBuilder para encadeamento
    }
}
```

2. **Atualize o `Program.cs`**:

```csharp
using MeuProjeto.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Uso da classe de extensão para logging
builder.Logging.AddCustomLogging();

builder.Services.AddControllers();
var app = builder.Build();

app.UseRouting();
app.MapControllers();
app.Run();
```

3. **Exemplo de uso em um serviço**:

```csharp
public class MeuServico
{
    private readonly ILogger<MeuServico> _logger;

    public MeuServico(ILogger<MeuServico> logger)
    {
        _logger = logger;
    }

    public void Executar()
    {
        _logger.LogTrace("Isso não aparecerá por causa do nível mínimo");
        _logger.LogInformation("Executando o serviço...");
        _logger.LogError("Erro simulado!");
    }
}
```

Nesse exemplo, apenas logs de nível `Information` ou superior serão exibidos, exceto para namespaces como `Microsoft` e `System`, que seguem os filtros definidos.

---

## Boas Práticas

1. **Escolha níveis apropriados**: Use `Debug` e `Trace` apenas em desenvolvimento; prefira `Information` para eventos normais e `Error`/`Critical` para falhas.
2. **Centralize a configuração**: Use classes de extensão ou arquivos de configuração (`appsettings.json`) para evitar código repetitivo.
3. **Adicione contexto**: Use mensagens de log descritivas e inclua variáveis com `{NomeDaVariavel}` para maior clareza.
   - Exemplo: `_logger.LogInformation("Usuário {UserId} logado", userId);`
4. **Considere provedores externos**: Para produção, integre ferramentas como Serilog ou NLog para logs em arquivos ou serviços como Azure Application Insights.

---

## Conclusão

O sistema de logging no ASP.NET Core é poderoso e fácil de usar, com suporte nativo a injeção de dependências e personalização via provedores e filtros. Usar uma classe de extensão para configurar o logging mantém o código organizado e escalável, permitindo ajustes centralizados. Com boas práticas, os logs podem ser uma ferramenta essencial para monitoramento e depuração.



