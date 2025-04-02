

# Tratamento Global de Exceções no ASP.NET Core

## Índice
1. [O que é Tratamento Global de Exceções?](#o-que-é-tratamento-global-de-exceções)
2. [Por que Usar?](#por-que-usar)
3. [Abordagens Disponíveis](#abordagens-disponíveis)
   - [Middleware de Exceção](#middleware-de-exceção)
   - [Filtro de Exceção Global](#filtro-de-exceção-global)
4. [Tutorial Passo a Passo](#tutorial-passo-a-passo)
   - [Passo 1: Implementar o Middleware](#passo-1-implementar-o-middleware)
   - [Passo 2: Implementar o Filtro Global](#passo-2-implementar-o-filtro-global)
   - [Passo 3: Configurar no Program.cs](#passo-3-configurar-no-programcs)
   - [Passo 4: Adicionar um Controller para Teste](#passo-4-adicionar-um-controller-para-teste)
   - [Passo 5: Testar as Abordagens](#passo-5-testar-as-abordagens)
5. [Boas Práticas](#boas-práticas)
6. [Conclusão](#conclusão)

---

## O que é Tratamento Global de Exceções?

O tratamento global de exceções captura e gerencia exceções não tratadas em toda a aplicação ASP.NET Core, centralizando a lógica em um único ponto, seja via middleware ou filtro.

---

## Por que Usar?

- Evita repetição de `try-catch` em cada *action*.
- Garante respostas consistentes (ex.: JSON com status HTTP).
- Facilita logging e monitoramento de erros.

---

## Abordagens Disponíveis

### Middleware de Exceção
- **Descrição**: Um componente no pipeline que captura exceções antes que cheguem ao cliente.
- **Vantagens**: Captura erros em todo o pipeline, incluindo fora de *controllers* (ex.: roteamento).
- **Quando usar**: Para controle total sobre exceções na aplicação.

### Filtro de Exceção Global
- **Descrição**: Um filtro MVC que trata exceções apenas dentro do escopo dos *controllers* e *actions*.
- **Vantagens**: Integra-se ao MVC, permitindo manipulação específica do contexto da ação.
- **Quando usar**: Quando o foco é apenas em erros gerados por *controllers*.

---

## Tutorial Passo a Passo

### Passo 1: Implementar o Middleware

Crie um middleware para tratamento global.

```csharp
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;

namespace MeuProjeto.Middlewares;

public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro não tratado: {Message}", ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";
        var (statusCode, message) = exception switch
        {
            KeyNotFoundException => (HttpStatusCode.NotFound, "Recurso não encontrado"),
            UnauthorizedAccessException => (HttpStatusCode.Unauthorized, "Acesso não autorizado"),
            ArgumentException => (HttpStatusCode.BadRequest, exception.Message),
            _ => (HttpStatusCode.InternalServerError, "Erro interno do servidor")
        };

        context.Response.StatusCode = (int)statusCode;
        var result = JsonSerializer.Serialize(new { Error = message, Timestamp = DateTime.UtcNow });
        return context.Response.WriteAsync(result);
    }
}
```

### Passo 2: Implementar o Filtro Global

Crie um filtro de exceções global.

```csharp
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using System;
using System.Net;

namespace MeuProjeto.Filters;

public class GlobalExceptionFilter : IExceptionFilter
{
    private readonly ILogger<GlobalExceptionFilter> _logger;

    public GlobalExceptionFilter(ILogger<GlobalExceptionFilter> logger)
    {
        _logger = logger;
    }

    public void OnException(ExceptionContext context)
    {
        _logger.LogError(context.Exception, "Erro no controller: {Message}", context.Exception.Message);

        var (statusCode, message) = context.Exception switch
        {
            KeyNotFoundException => (HttpStatusCode.NotFound, "Recurso não encontrado"),
            UnauthorizedAccessException => (HttpStatusCode.Unauthorized, "Acesso não autorizado"),
            ArgumentException => (HttpStatusCode.BadRequest, context.Exception.Message),
            _ => (HttpStatusCode.InternalServerError, "Erro interno do servidor")
        };

        context.Result = new ObjectResult(new { Error = message, Timestamp = DateTime.UtcNow })
        {
            StatusCode = (int)statusCode
        };
        context.ExceptionHandled = true;
    }
}
```

### Passo 3: Configurar no Program.cs

Configure ambas as abordagens (você pode escolher uma ou usar as duas).

```csharp
using MeuProjeto.Filters;
using MeuProjeto.Middlewares;

var builder = WebApplication.CreateBuilder(args);

// Adiciona logging e controllers
builder.Services.AddLogging(logging => logging.AddConsole());
builder.Services.AddControllers(options =>
{
    // Registra o filtro global
    options.Filters.Add<GlobalExceptionFilter>();
});

var app = builder.Build();

// Adiciona o middleware de exceção (captura tudo)
app.UseMiddleware<GlobalExceptionMiddleware>();

app.UseRouting();
app.MapControllers();

app.Run();
```

### Passo 4: Adicionar um Controller para Teste

Crie um *Controller* para simular exceções.

```csharp
using Microsoft.AspNetCore.Mvc;

namespace MeuProjeto.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TesteController : ControllerBase
{
    [HttpGet("not-found")]
    public Task GetNotFound() => throw new KeyNotFoundException("Item não encontrado");

    [HttpGet("unauthorized")]
    public Task GetUnauthorized() => throw new UnauthorizedAccessException("Sem permissão");

    [HttpGet("bad-request")]
    public Task GetBadRequest() => throw new ArgumentException("Parâmetro inválido");

    [HttpGet("server-error")]
    public Task GetServerError() => throw new Exception("Erro inesperado");
}
```

### Passo 5: Testar as Abordagens

- **GET /api/teste/not-found**:
  - Resposta (middleware ou filtro): `404 Not Found` com `{"Error": "Recurso não encontrado", "Timestamp": "2025-04-01T00:00:00Z"}`
- **GET /api/teste/unauthorized**:
  - Resposta: `401 Unauthorized` com `{"Error": "Acesso não autorizado", "Timestamp": "2025-04-01T00:00:00Z"}`
- **GET /api/teste/bad-request**:
  - Resposta: `400 Bad Request` com `{"Error": "Parâmetro inválido", "Timestamp": "2025-04-01T00:00:00Z"}`
- **GET /api/teste/server-error**:
  - Resposta: `500 Internal Server Error` com `{"Error": "Erro interno do servidor", "Timestamp": "2025-04-01T00:00:00Z"}`

> **Nota**: O middleware captura todas as exceções no pipeline, enquanto o filtro só atua em exceções geradas dentro de *controllers*. Se ambos estiverem ativos, o middleware interceptará primeiro.

---

## Boas Práticas

1. **Escolha a abordagem certa**: Use middleware para cobertura total do pipeline; use filtros para tratamento específico de *controllers*.
2. **Posicionamento**: Coloque o middleware no início do pipeline para capturar tudo.
3. **Respostas consistentes**: Padronize o formato JSON (ex.: `{ "Error": "", "Timestamp": "" }`).
4. **Logging**: Registre detalhes no `ILogger`, mas evite expor stack traces em produção.
5. **Teste exaustivo**: Simule diferentes tipos de exceções para validar o comportamento.

---

## Conclusão

O tratamento global de exceções no ASP.NET Core pode ser implementado via middleware ou filtros globais, cada um com vantagens específicas. O middleware oferece cobertura ampla, enquanto o filtro é mais integrado ao MVC. Este tutorial demonstra ambas as abordagens, usando `async/await` no middleware e configurando respostas consistentes. A escolha depende do escopo desejado: pipeline completo (middleware) ou apenas *controllers* (filtro).

