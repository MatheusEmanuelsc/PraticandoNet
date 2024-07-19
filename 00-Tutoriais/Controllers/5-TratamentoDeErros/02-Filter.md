# Filtros no ASP.NET Core

## Índice
1. [Introdução aos Filtros](#introdução-aos-filtros)
2. [Tipos de Filtros](#tipos-de-filtros)
   - [Filtros de Autorização](#filtros-de-autorização)
   - [Filtros de Recurso](#filtros-de-recurso)
   - [Filtros de Ação](#filtros-de-ação)
   - [Filtros de Exceção](#filtros-de-exceção)
   - [Filtros de Resultado](#filtros-de-resultado)
3. [Implementação de Filtros](#implementação-de-filtros)
   - [Criando um Filtro de Ação](#criando-um-filtro-de-ação)
   - [Registrando Filtros](#registrando-filtros)
   - [Aplicando Filtros Globalmente](#aplicando-filtros-globalmente)
4. [Exemplos Práticos](#exemplos-práticos)
5. [Tutorial Passo a Passo](#tutorial-passo-a-passo)
   - [Passo 1: Configuração do Projeto](#passo-1-configuração-do-projeto)
   - [Passo 2: Criando um Filtro Personalizado](#passo-2-criando-um-filtro-personalizado)
   - [Passo 3: Registrando e Aplicando o Filtro](#passo-3-registrando-e-aplicando-o-filtro)

---

## Introdução aos Filtros

Os filtros no ASP.NET Core são componentes que permitem a execução de código antes ou depois de determinadas fases da pipeline de execução de uma requisição. Eles são utilizados para aspectos transversais, como autenticação, autorização, log, tratamento de exceções, etc.

## Tipos de Filtros

### Filtros de Autorização
Os filtros de autorização controlam o acesso ao código do controlador e ao método de ação. Eles são executados antes de qualquer outro filtro.

### Filtros de Recurso
Os filtros de recurso lidam com a inicialização e finalização de recursos ao redor da execução de uma ação. Eles são úteis para gerenciar conexões de banco de dados ou outras operações que precisam de início e fim claros.

### Filtros de Ação
Os filtros de ação são executados antes e depois do método de ação ser invocado. Eles podem ser usados para modificar argumentos de ação ou o resultado da ação.

### Filtros de Exceção
Os filtros de exceção são executados quando uma exceção não tratada é lançada dentro do pipeline de processamento da requisição.

### Filtros de Resultado
Os filtros de resultado são executados antes e depois do resultado de uma ação ser processado. Eles podem ser usados para modificar o resultado final antes de ser enviado ao cliente.

## Implementação de Filtros

### Criando um Filtro de Ação

Para criar um filtro de ação, você precisa implementar a interface `IActionFilter` ou herdar de `ActionFilterAttribute`.

```csharp
public class SampleActionFilter : IActionFilter
{
    public void OnActionExecuting(ActionExecutingContext context)
    {
        // Código antes da ação
    }

    public void OnActionExecuted(ActionExecutedContext context)
    {
        // Código após a ação
    }
}
```

### Registrando Filtros

Você pode registrar filtros de várias maneiras, incluindo diretamente no controlador ou método de ação, ou globalmente na configuração do MVC.

```csharp
public void ConfigureServices(IServiceCollection services)
{
    services.AddControllers(options =>
    {
        options.Filters.Add<SampleActionFilter>();
    });
}
```

### Aplicando Filtros Globalmente

Filtros globais são aplicados a todas as ações e controladores.

```csharp
services.AddControllers(options =>
{
    options.Filters.Add(new SampleActionFilter());
});
```

## Exemplos Práticos

Aqui está um exemplo de um filtro de ação que loga informações antes e depois da execução de uma ação:

```csharp
public class LoggingActionFilter : IActionFilter
{
    private readonly ILogger<LoggingActionFilter> _logger;

    public LoggingActionFilter(ILogger<LoggingActionFilter> logger)
    {
        _logger = logger;
    }

    public void OnActionExecuting(ActionExecutingContext context)
    {
        _logger.LogInformation("Executing action {ActionName}", context.ActionDescriptor.DisplayName);
    }

    public void OnActionExecuted(ActionExecutedContext context)
    {
        _logger.LogInformation("Executed action {ActionName}", context.ActionDescriptor.DisplayName);
    }
}
```

## Tutorial Passo a Passo

### Passo 1: Configuração do Projeto

1. Crie um novo projeto ASP.NET Core.
2. Configure os serviços e middlewares necessários no arquivo `Startup.cs`.

### Passo 2: Criando um Filtro Personalizado

1. Crie uma nova classe que implemente `IActionFilter` ou herde de `ActionFilterAttribute`.

```csharp
public class CustomActionFilter : IActionFilter
{
    public void OnActionExecuting(ActionExecutingContext context)
    {
        // Código executado antes da ação
    }

    public void OnActionExecuted(ActionExecutedContext context)
    {
        // Código executado depois da ação
    }
}
```

### Passo 3: Registrando e Aplicando o Filtro

1. Registre o filtro na configuração de serviços no `Startup.cs`.

```csharp
public void ConfigureServices(IServiceCollection services)
{
    services.AddControllers(options =>
    {
        options.Filters.Add<CustomActionFilter>();
    });
}
```

2. Opcionalmente, aplique o filtro a um controlador ou ação específica.

```csharp
[ServiceFilter(typeof(CustomActionFilter))]
public class HomeController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}
```

Seguindo estes passos, você conseguirá configurar e utilizar filtros personalizados em seu projeto ASP.NET Core.