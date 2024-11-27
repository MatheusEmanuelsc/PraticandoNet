
## Índice

1. [Introdução](#introdução)
2. [Etapa 1: Criar a Classe `ErrorDetails`](#etapa-1-criar-a-classe-errordetails)
3. [Etapa 2: Criar o Método de Extensão `ConfigureExceptionHandler`](#etapa-2-criar-o-método-de-extensão-configureexceptionhandler)
4. [Etapa 3: Configurar o Middleware de Tratamento de Erros](#etapa-3-configurar-o-middleware-de-tratamento-de-erros)
5. [Etapa 4: Habilitar o Uso do Método de Extensão na Classe `Program`](#etapa-4-habilitar-o-uso-do-método-de-extensão-na-classe-program)
6. [Conclusão](#conclusão)

## Introdução

Este tutorial mostra como configurar o tratamento de erros de maneira profissional em uma Web API usando .NET 8. A implementação inclui a criação de uma classe para representar os detalhes dos erros, um método de extensão para configurar o tratamento de exceções, e a configuração do middleware de tratamento de erros na classe `Program`.

## Etapa 1: Criar a Classe `ErrorDetails`

Crie uma classe para representar os detalhes dos erros no projeto.

```csharp
using System.Text.Json;

namespace Curso.Api.Exceptions
{
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
}
```

Esta classe contém três propriedades: `StatusCode`, `Message` e `Trace`. O método `ToString` sobrescrito serializa a instância da classe em formato JSON.

## Etapa 2: Criar o Método de Extensão `ConfigureExceptionHandler`

Crie um método de extensão para configurar o tratamento de exceções no pipeline de middleware.

```csharp
using Microsoft.AspNetCore.Diagnostics;
using System.Net;

namespace Curso.Api.Exceptions.Extensions
{
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
}
```

Este método configura o middleware `UseExceptionHandler` para capturar todas as exceções não tratadas e retornar uma resposta JSON com os detalhes do erro.

## Etapa 3: Configurar o Middleware de Tratamento de Erros

Configure o middleware de tratamento de erros no pipeline de middleware da aplicação.

```csharp
public class Startup
{
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }
        else
        {
            app.UseHsts();
        }

        app.ConfigureExceptionHandler(); // Configurar tratamento de erros

        app.UseHttpsRedirection();
        app.UseRouting();
        app.UseAuthorization();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
        });
    }
}
```

## Etapa 4: Habilitar o Uso do Método de Extensão na Classe `Program`

Certifique-se de habilitar o uso do método de extensão na classe `Program`.

```csharp
using Curso.Api.Exceptions.Extensions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

public class Program
{
    public static void Main(string[] args)
    {
        CreateHostBuilder(args).Build().Run();
    }

    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseStartup<Startup>();
            });
}
```

## Conclusão

Neste tutorial, implementamos um tratamento de erros robusto para uma Web API em .NET 8. Criamos uma classe para representar os detalhes dos erros, configuramos um método de extensão para capturar exceções não tratadas e configuramos o middleware de tratamento de erros na classe `Program`. Esta abordagem melhora a manutenibilidade e clareza do código, garantindo respostas consistentes para erros inesperados.

