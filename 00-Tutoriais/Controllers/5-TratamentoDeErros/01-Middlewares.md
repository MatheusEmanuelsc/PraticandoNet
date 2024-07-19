```markdown
# Tutorial: Middleware

## Índice

1. [O que é Middleware?](#o-que-é-middleware)
2. [Tratamento de Erros com Middleware](#tratamento-de-erros-com-middleware)
3. [Ordem dos Middlewares](#ordem-dos-middlewares)
   - [Exemplo de Middleware Predefinido](#exemplo-de-middleware-predefinido)
   - [Exemplo de Middleware Personalizado](#exemplo-de-middleware-personalizado)
4. [Tratamento Global de Exceções com Middleware](#tratamento-global-de-exceções-com-middleware)
   - [Criar a Entidade `ErrorDetails`](#criar-a-entidade-errordetails)
   - [Método de Extensão `ConfigureExceptionHandler`](#método-de-extensão-configureexceptionhandler)
   - [Habilitar o Uso do Método de Extensão na Classe `Program`](#habilitar-o-uso-do-método-de-extensão-na-classe-program)
   - [Testar a Implementação](#testar-a-implementação)

## O que é Middleware?

Middleware é um componente de software que trata requisições e respostas no pipeline de processamento de um aplicativo web. Ele pode realizar várias tarefas, como autenticação, logging, manipulação de erros, entre outras, antes que a requisição seja passada para a próxima etapa do pipeline.

## Tratamento de Erros com Middleware

Utilizar middleware para tratamento de erros permite centralizar a lógica de captura e manipulação de exceções, facilitando a manutenção e a padronização das respostas de erro do aplicativo.

## Ordem dos Middlewares

A ordem dos middlewares no pipeline de processamento é crucial. Middlewares são executados na ordem em que são registrados, e a sequência pode afetar o comportamento e o fluxo das requisições.

### Exemplo de Middleware Predefinido

1. **UseAuthentication**

   ```csharp
   app.UseAuthentication();
   ```

2. **UseAuthorization**

   ```csharp
   app.UseAuthorization();
   ```

### Exemplo de Middleware Personalizado

1. **Logging Middleware**

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
           Console.WriteLine($"Response: {context.Response.StatusCode}");
       }
   }
   ```

2. **Custom Header Middleware**

   ```csharp
   public class CustomHeaderMiddleware
   {
       private readonly RequestDelegate _next;

       public CustomHeaderMiddleware(RequestDelegate next)
       {
           _next = next;
       }

       public async Task InvokeAsync(HttpContext context)
       {
           context.Response.OnStarting(() =>
           {
               context.Response.Headers.Add("X-Custom-Header", "Middleware Demo");
               return Task.CompletedTask;
           });

           await _next(context);
       }
   }
   ```

## Tratamento Global de Exceções com Middleware

### Criar a Entidade `ErrorDetails`

```csharp
public class ErrorDetails
{
    public int StatusCode { get; set; }
    public string? Message { get; set; }
    public string? Trace { get; set; }

    public override string ToString()
    {
        return JsonSerializer.Serialize(this);
    }
}
```

### Método de Extensão `ConfigureExceptionHandler`

```csharp
public static class ApiExceptionMiddlewareExtensions
{
    public static void ConfigureExceptionHandler(this IApplicationBuilder app)
    {
        app.UseExceptionHandler(appError =>
        {
            appError.Run(async context =>
            {
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                context.Response.ContentType = "application/json";

                var contextFeature = context.Features.Get<IExceptionHandlerFeature>();
                if (contextFeature != null)
                {
                    await context.Response.WriteAsync(new ErrorDetails()
                    {
                        StatusCode = context.Response.StatusCode,
                        Message = contextFeature.Error.Message,
                        Trace = contextFeature.Error.StackTrace
                    }.ToString());
                }
            });
        });
    }
}
```

### Habilitar o Uso do Método de Extensão na Classe `Program`

```csharp
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.ConfigureExceptionHandler();
}
```

### Testar a Implementação

Para testar a implementação, execute a aplicação e force um erro para verificar se o middleware de tratamento de exceções está funcionando corretamente e retornando as respostas padronizadas.
```