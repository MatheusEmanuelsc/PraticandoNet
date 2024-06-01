Com a introdução do .NET 6, a classe `Startup` foi removida em favor de um novo modelo de inicialização mínima. Vamos adaptar o conteúdo para refletir essa mudança.

# Middlewares no ASP.NET Core

## Índice

1. [Introdução aos Middlewares](#introducao-aos-middlewares)
2. [Exemplos de Middlewares Comuns](#exemplos-de-middlewares-comuns)
3. [Criando um Middleware Personalizado](#criando-um-middleware-personalizado)
4. [Exemplo de Tratamento de Erros com Middleware](#exemplo-de-tratamento-de-erros-com-middleware)

## Introdução aos Middlewares

Middlewares são componentes que formam o pipeline de requisição/resposta no ASP.NET Core. Cada middleware no pipeline é responsável por processar a requisição e decidir se passa a requisição para o próximo middleware ou se encerra o processamento.

No .NET 6, o pipeline de middlewares é configurado diretamente no método `Main` da classe `Program`.

```csharp
var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.UseMiddleware<FirstMiddleware>();
app.UseMiddleware<SecondMiddleware>();

app.Run(async (context) =>
{
    await context.Response.WriteAsync("Hello, world!");
});

app.Run();
```

## Exemplos de Middlewares Comuns

### Logging Middleware

```csharp
public class LoggingMiddleware
{
    private readonly RequestDelegate _next;

    public LoggingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        Console.WriteLine($"Request: {context.Request.Method} {context.Request.Path}");
        await _next(context);
    }
}
```

### Authentication Middleware

```csharp
public class AuthenticationMiddleware
{
    private readonly RequestDelegate _next;

    public AuthenticationMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (!context.User.Identity.IsAuthenticated)
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsync("Unauthorized");
            return;
        }
        await _next(context);
    }
}
```

## Criando um Middleware Personalizado

Para criar um middleware personalizado no ASP.NET Core, você precisa seguir três passos principais:

1. **Criar a Classe do Middleware**
2. **Adicionar a Extensão de Middleware**
3. **Configurar o Middleware no Pipeline**

### Passo 1: Criar a Classe do Middleware

Crie uma nova classe que implementa a lógica do middleware. O middleware deve incluir um construtor que aceite um `RequestDelegate` e um método `Invoke` ou `InvokeAsync` que faça o processamento da requisição.

```csharp
public class CustomMiddleware
{
    private readonly RequestDelegate _next;

    public CustomMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Lógica do middleware
        Console.WriteLine("Custom Middleware Executing");

        await _next(context); // Chama o próximo middleware no pipeline

        // Lógica após o próximo middleware
        Console.WriteLine("Custom Middleware Executed");
    }
}
```

### Passo 2: Adicionar a Extensão de Middleware

Para facilitar a adição do middleware ao pipeline, crie um método de extensão.

```csharp
public static class CustomMiddlewareExtensions
{
    public static IApplicationBuilder UseCustomMiddleware(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<CustomMiddleware>();
    }
}
```

### Passo 3: Configurar o Middleware no Pipeline

No método `Main` da classe `Program`, adicione o middleware personalizado ao pipeline.

```csharp
var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.UseCustomMiddleware();
// Outros middlewares

app.Run();
```

## Exemplo de Tratamento de Erros com Middleware

Um dos casos de uso comuns para middlewares é o tratamento centralizado de erros. Vamos criar um middleware que captura exceções e retorna uma resposta de erro amigável.

### Passo 1: Criar a Classe do Middleware de Tratamento de Erros

```csharp
public class ErrorHandlingMiddleware
{
    private readonly RequestDelegate _next;

    public ErrorHandlingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

        var response = new { message = "An unexpected error occurred", details = exception.Message };
        return context.Response.WriteAsync(JsonConvert.SerializeObject(response));
    }
}
```

### Passo 2: Adicionar a Extensão de Middleware

```csharp
public static class ErrorHandlingMiddlewareExtensions
{
    public static IApplicationBuilder UseErrorHandlingMiddleware(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<ErrorHandlingMiddleware>();
    }
}
```

### Passo 3: Configurar o Middleware no Pipeline

Adicione o middleware de tratamento de erros no início do pipeline para garantir que todas as exceções sejam capturadas.

```csharp
var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.UseErrorHandlingMiddleware();
// Outros middlewares

app.Run();
```

### Passo 4: Testando o Middleware de Tratamento de Erros

Crie um controlador que gera uma exceção para testar o middleware.

```csharp
[ApiController]
[Route("api/[controller]")]
public class TestController : ControllerBase
{
    [HttpGet("error")]
    public IActionResult GetError()
    {
        throw new Exception("This is a test exception");
    }
}
```

Quando você acessar a rota `api/test/error`, o middleware de tratamento de erros deve capturar a exceção e retornar uma resposta JSON com a mensagem de erro.

```json
{
    "message": "An unexpected error occurred",
    "details": "This is a test exception"
}
```

Com isso, você tem um middleware de tratamento de erros centralizado que melhora a robustez e a experiência de depuração da sua aplicação ASP.NET Core.