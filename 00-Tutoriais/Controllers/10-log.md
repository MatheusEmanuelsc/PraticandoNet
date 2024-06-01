# Logging no ASP.NET Core

## Índice

- [Introdução](#introdução)
- [Configuração Básica](#configuração-básica)
- [Provedores de Log](#provedores-de-log)
  - [Console](#console)
  - [Debug](#debug)
  - [EventSource](#eventsource)
  - [EventLog (Windows)](#eventlog-windows)
  - [TraceSource](#tracesource)
- [Níveis de Log](#níveis-de-log)
- [Filtros de Log](#filtros-de-log)
- [Exemplo Prático](#exemplo-prático)
- [Tutorial Completo](#tutorial-completo)
  - [Passo 1: Criar um Projeto ASP.NET Core](#passo-1-criar-um-projeto-aspnet-core)
  - [Passo 2: Configurar o Logging no `appsettings.json`](#passo-2-configurar-o-logging-no-appsettingsjson)
  - [Passo 3: Adicionar Provedores de Log no `Program.cs`](#passo-3-adicionar-provedores-de-log-no-programcs)
  - [Passo 4: Injetar e Utilizar o Logger em Controladores](#passo-4-injetar-e-utilizar-o-logger-em-controladores)

## Introdução

O logging é uma funcionalidade essencial para a maioria das aplicações, permitindo que desenvolvedores capturem informações sobre a execução da aplicação para diagnóstico e análise. No ASP.NET Core, o sistema de logging é altamente configurável e permite a integração com diversos provedores de log.

## Configuração Básica

O ASP.NET Core oferece suporte a logging embutido, permitindo que os desenvolvedores configurem e utilizem o logging de forma fácil e eficiente. A configuração básica pode ser realizada no arquivo `appsettings.json` e no `Program.cs`.

## Provedores de Log

ASP.NET Core suporta vários provedores de log, cada um permitindo que logs sejam enviados para diferentes destinos.

### Console

Este provedor envia logs para o console.

```csharp
logging.AddConsole();
```

### Debug

Este provedor envia logs para o debugger.

```csharp
logging.AddDebug();
```

### EventSource

Este provedor envia logs para um EventSource, útil para ferramentas de monitoramento.

```csharp
logging.AddEventSourceLogger();
```

### EventLog (Windows)

Este provedor envia logs para o Windows Event Log, disponível apenas em Windows.

```csharp
logging.AddEventLog();
```

### TraceSource

Este provedor envia logs para `System.Diagnostics.TraceSource` instances.

```csharp
logging.AddTraceSource("TraceSourceName");
```

## Níveis de Log

Os níveis de log ajudam a categorizar a severidade das mensagens de log. Os níveis padrão são:

- Trace
- Debug
- Information
- Warning
- Error
- Critical
- None

## Filtros de Log

Os filtros de log permitem controlar quais mensagens de log são registradas com base no nível de log e na categoria.

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning",
      "Microsoft.Hosting.Lifetime": "Information"
    }
  }
}
```

## Exemplo Prático

Aqui está um exemplo prático de como configurar e usar o logging no ASP.NET Core:

```csharp
public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    public IActionResult Index()
    {
        _logger.LogInformation("Acessando a Home Page.");
        return View();
    }

    public IActionResult Privacy()
    {
        _logger.LogWarning("Página de privacidade acessada.");
        return View();
    }
}
```

## Tutorial Completo

### Passo 1: Criar um Projeto ASP.NET Core

Abra o terminal e crie um novo projeto ASP.NET Core:

```bash
dotnet new mvc -n LoggingExample
```

### Passo 2: Configurar o Logging no `appsettings.json`

Edite o arquivo `appsettings.json` para adicionar configurações de logging:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning",
      "Microsoft.Hosting.Lifetime": "Information"
    }
  }
}
```

### Passo 3: Adicionar Provedores de Log no `Program.cs`

No arquivo `Program.cs`, configure os provedores de log:

```csharp
public class Program
{
    public static void Main(string[] args)
    {
        CreateHostBuilder(args).Build().Run();
    }

    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureLogging(logging =>
            {
                logging.ClearProviders();
                logging.AddConsole();
                logging.AddDebug();
            })
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseStartup<Startup>();
            });
}
```

### Passo 4: Injetar e Utilizar o Logger em Controladores

Injete o `ILogger` nos controladores e utilize-o:

```csharp
public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    public IActionResult Index()
    {
        _logger.LogInformation("Acessando a Home Page.");
        return View();
    }

    public IActionResult Privacy()
    {
        _logger.LogWarning("Página de privacidade acessada.");
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        _logger.LogError("Ocorreu um erro.");
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
```

Com essas etapas, você configurou e integrou com sucesso o sistema de logging no seu projeto ASP.NET Core.