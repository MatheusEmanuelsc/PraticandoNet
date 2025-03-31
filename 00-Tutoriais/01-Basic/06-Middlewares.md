
---

# Middlewares no ASP.NET Core

## Índice
1. [O que são Middlewares?](#o-que-são-middlewares)
2. [Middlewares no ASP.NET Core](#middlewares-no-aspnet-core)
   - [Pipeline de Requisição](#pipeline-de-requisição)
   - [Tipos de Middlewares](#tipos-de-middlewares)
3. [Configurando Middlewares](#configurando-middlewares)
   - [Métodos Básicos](#métodos-básicos)
   - [Exemplo Simples](#exemplo-simples)
4. [Criando uma Classe de Extensão para Middlewares](#criando-uma-classe-de-extensão-para-middlewares)
   - [Por que usar uma classe de extensão?](#por-que-usar-uma-classe-de-extensão)
   - [Exemplo com Classe de Extensão](#exemplo-com-classe-de-extensão)
5. [Boas Práticas](#boas-práticas)
6. [Conclusão](#conclusão)

---

## O que são Middlewares?

Middlewares são componentes que formam o pipeline de processamento de requisições HTTP no ASP.NET Core. Eles interceptam, processam ou modificam requisições e respostas, permitindo funcionalidades como autenticação, logging, roteamento, tratamento de erros, entre outros.

---

## Middlewares no ASP.NET Core

### Pipeline de Requisição
O pipeline de middlewares é uma sequência de componentes configurados no `Program.cs`. Cada middleware pode:
- Processar a requisição e passar para o próximo (`next()`).
- Interromper o fluxo (short-circuit) e retornar uma resposta diretamente.
- Modificar a requisição ou resposta antes ou depois de chamar o próximo middleware.

A ordem de configuração no pipeline é crucial, pois determina a sequência de execução.

### Tipos de Middlewares
- **Built-in**: Fornecidos pelo ASP.NET Core, como `UseRouting`, `UseAuthentication`, `UseAuthorization`.
- **Personalizados**: Criados pelo desenvolvedor para atender a necessidades específicas.
- **Terceiros**: Disponíveis via pacotes NuGet, como o middleware de CORS ou logging.

---

## Configurando Middlewares

### Métodos Básicos
Os métodos principais para configurar middlewares são:
- `Use`: Adiciona um middleware ao pipeline e permite chamar o próximo.
- `Run`: Adiciona um middleware terminal (não chama o próximo).
- `Map`: Ramifica o pipeline com base em uma condição (ex.: URL).

### Exemplo Simples
Aqui está um exemplo básico configurando middlewares no `Program.cs`:

```csharp
var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

// Middleware simples
app.Use(async (context, next) =>
{
    await context.Response.WriteAsync("Middleware 1: Antes\n");
    await next(); // Chama o próximo middleware
    await context.Response.WriteAsync("Middleware 1: Depois\n");
});

// Middleware terminal
app.Run(async context =>
{
    await context.Response.WriteAsync("Middleware 2: Terminal\n");
});

app.Run();
```

Ao acessar a aplicação, a saída será:
```
Middleware 1: Antes
Middleware 2: Terminal
Middleware 1: Depois
```

---

## Criando uma Classe de Extensão para Middlewares

### Por que usar uma classe de extensão?
Com o crescimento da aplicação, o `Program.cs` pode ficar sobrecarregado com muitos middlewares. Uma classe de extensão organiza a configuração do pipeline, melhora a legibilidade e permite reutilização.

### Exemplo com Classe de Extensão
Vamos criar uma classe de extensão para configurar middlewares personalizados.

1. **Crie a classe de extensão**:

```csharp
namespace MeuProjeto.Extensions;

public static class MiddlewareExtensions
{
    public static IApplicationBuilder UseCustomMiddlewares(this IApplicationBuilder app)
    {
        // Middleware de logging customizado
        app.Use(async (context, next) =>
        {
            var startTime = DateTime.Now;
            await next();
            var duration = DateTime.Now - startTime;
            await context.Response.WriteAsync($"[LOG] Requisição levou {duration.TotalMilliseconds}ms\n");
        });

        // Middleware de cabeçalho customizado
        app.Use(async (context, next) =>
        {
            context.Response.Headers.Add("X-Custom-Header", "MeuProjeto");
            await next();
        });

        return app; // Retorna o IApplicationBuilder para encadeamento
    }
}
```

2. **Atualize o `Program.cs`**:

```csharp
using MeuProjeto.Extensions;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

// Uso da classe de extensão
app.UseCustomMiddlewares();

// Middleware padrão
app.UseRouting();
app.MapGet("/", () => "Hello, World!");

app.Run();
```

3. **Exemplo de saída**:
Ao acessar `/`, a resposta será:
```
Hello, World![LOG] Requisição levou 5.123ms
```
E o cabeçalho `X-Custom-Header: MeuProjeto` estará presente na resposta HTTP.

---

## Boas Práticas

1. **Ordem importa**: Configure os middlewares na ordem correta (ex.: `UseRouting` antes de `UseEndpoints`, `UseAuthentication` antes de `UseAuthorization`).
2. **Evite lógica pesada**: Middlewares devem ser leves; delegue tarefas complexas a serviços injetados.
3. **Use condicionais com moderação**: Prefira `Map` ou `MapWhen` para ramificações ao invés de `if` dentro de um middleware.
4. **Teste o pipeline**: Verifique o comportamento do pipeline em diferentes cenários (ex.: erros, requisições inválidas).

---

## Conclusão

Middlewares no ASP.NET Core são a espinha dorsal do processamento de requisições, oferecendo flexibilidade para personalizar o fluxo da aplicação. Usar uma classe de extensão para organizar middlewares personalizados mantém o código limpo e modular, facilitando a manutenção e expansão. Com uma configuração cuidadosa e boas práticas, o pipeline pode atender às mais diversas necessidades de uma aplicação.

