

# Tratamento de Exceções Global no ASP.NET Core

## Índice
1. [O que é Tratamento de Exceções Global?](#o-que-é-tratamento-de-exceções-global)
2. [Por que Usar?](#por-que-usar)
3. [Tutorial Passo a Passo](#tutorial-passo-a-passo)
   - [Passo 1: Criar o Middleware de Exceção](#passo-1-criar-o-middleware-de-exceção)
   - [Passo 2: Criar uma Classe de Extensão](#passo-2-criar-uma-classe-de-extensão)
   - [Passo 3: Configurar no Program.cs](#passo-3-configurar-no-programcs)
   - [Passo 4: Testar o Tratamento](#passo-4-testar-o-tratamento)
4. [Boas Práticas](#boas-práticas)
5. [Conclusão](#conclusão)

---

## O que é Tratamento de Exceções Global?

O tratamento de exceções global é uma abordagem para capturar e gerenciar exceções não tratadas em toda a aplicação ASP.NET Core, centralizando a lógica em um único ponto, geralmente um middleware.

---

## Por que Usar?

- Evita repetição de try-catch em cada endpoint.
- Garante respostas consistentes para erros (ex.: JSON com status HTTP apropriado).
- Facilita logging e monitoramento de erros.

---

## Tutorial Passo a Passo

### Passo 1: Criar o Middleware de Exceção

Crie uma classe para o middleware que captura exceções e retorna uma resposta padronizada.

```csharp
namespace MeuProjeto.Middlewares;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;

    public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
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
            _logger.LogError(ex, "Ocorreu um erro não tratado: {Message}", ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";
        var (statusCode, message) = exception switch
        {
            KeyNotFoundException => (StatusCodes.Status404NotFound, "Recurso não encontrado"),
            UnauthorizedAccessException => (StatusCodes.Status401Unauthorized, "Acesso não autorizado"),
            _ => (StatusCodes.Status500InternalServerError, "Erro interno do servidor")
        };

        context.Response.StatusCode = statusCode;
        var result = JsonSerializer.Serialize(new { error = message });
        return context.Response.WriteAsync(result);
    }
}
```

### Passo 2: Criar uma Classe de Extensão

Crie uma extensão para facilitar o uso do middleware no pipeline.

```csharp
namespace MeuProjeto.Extensions;

public static class ExceptionMiddlewareExtensions
{
    public static IApplicationBuilder UseGlobalExceptionHandler(this IApplicationBuilder app)
    {
        return app.UseMiddleware<ExceptionMiddleware>();
    }
}
```

### Passo 3: Configurar no Program.cs

Adicione o middleware ao pipeline no `Program.cs`.

```csharp
using MeuProjeto.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Configuração de serviços
builder.Services.AddControllers();

// Configuração de logging
builder.Logging.AddConsole();

var app = builder.Build();

// Adiciona o middleware de exceção no início do pipeline
app.UseGlobalExceptionHandler();

// Outros middlewares
app.UseRouting();
app.MapControllers();

app.Run();
```

### Passo 4: Testar o Tratamento

Adicione um endpoint para simular uma exceção e teste.

```csharp
public class TestController : ControllerBase
{
    [HttpGet("test-error")]
    public IActionResult TestError()
    {
        throw new KeyNotFoundException("Item não encontrado");
        return Ok("Sucesso");
    }

    [HttpGet("test-unauthorized")]
    public IActionResult TestUnauthorized()
    {
        throw new UnauthorizedAccessException("Sem permissão");
        return Ok("Sucesso");
    }
}
```

- Acesse `/test-error`: Retorna `404` com `{"error": "Recurso não encontrado"}`.
- Acesse `/test-unauthorized`: Retorna `401` com `{"error": "Acesso não autorizado"}`.
- Qualquer outra exceção retorna `500` com `{"error": "Erro interno do servidor"}`.

O erro também será registrado no console pelo `ILogger`.

---

## Boas Práticas

1. **Posicione cedo no pipeline**: Coloque o middleware de exceção antes de outros middlewares para capturar erros em qualquer etapa.
2. **Personalize respostas**: Adapte os códigos de status e mensagens conforme os tipos de exceção.
3. **Registre detalhes**: Use o logger para incluir stack trace e contexto da requisição, se necessário.
4. **Evite expor detalhes sensíveis**: Não retorne stack traces ou informações internas em ambientes de produção.
5. **Teste cenários**: Simule diferentes exceções para garantir que o tratamento funciona como esperado.

---

## Conclusão

O tratamento de exceções global com um middleware no ASP.NET Core é uma solução eficiente para centralizar a gestão de erros, oferecendo respostas consistentes e logging integrado. Usar uma classe de extensão facilita a configuração e mantém o `Program.cs` organizado. Com este tutorial, você pode implementar rapidamente um handler robusto e escalável.
