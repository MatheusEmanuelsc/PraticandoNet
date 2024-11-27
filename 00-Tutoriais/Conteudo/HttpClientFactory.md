### Tutorial Completo e Resumo sobre `HttpClientFactory` no .NET 8

O `HttpClientFactory` é uma ferramenta poderosa introduzida no .NET Core para resolver problemas comuns ao usar `HttpClient`, como o gerenciamento inadequado de conexões e o suporte a reuso e configuração de clientes HTTP. Neste tutorial, você aprenderá como configurar e usar o `HttpClientFactory` para realizar chamadas HTTP de maneira eficiente e segura.

## Índice

1. [Introdução ao HttpClientFactory](#introdução-ao-httpclientfactory)
2. [Por que usar o HttpClientFactory?](#por-que-usar-o-httpclientfactory)
3. [Tipos de HttpClient no HttpClientFactory](#tipos-de-httpclient-no-httpclientfactory)
   - [1. Basic Usage](#1-basic-usage)
   - [2. Named Clients](#2-named-clients)
   - [3. Typed Clients](#3-typed-clients)
   - [4. HttpClient com Handlers](#4-httpclient-com-handlers)
4. [Implementando HttpClientFactory em uma Aplicação ASP.NET Core](#implementando-httpclientfactory-em-uma-aplicação-aspnet-core)
5. [Configurando Políticas de Resiliência com Polly](#configurando-políticas-de-resiliência-com-polly)
6. [Resumo](#resumo)

---

## Introdução ao HttpClientFactory

O `HttpClientFactory` é um recurso do .NET Core que facilita a criação e configuração de instâncias de `HttpClient`. Ele é responsável por:

- Gerenciar o ciclo de vida das instâncias de `HttpClient`.
- Evitar problemas comuns de uso, como esgotamento de conexões TCP.
- Fornecer uma maneira centralizada de aplicar políticas de resiliência, como Retry e Circuit Breaker.

## Por que usar o HttpClientFactory?

### Benefícios:

1. **Gerenciamento de Conexões**: Elimina problemas de esgotamento de conexões TCP e evita o uso incorreto do `HttpClient`.
2. **Configuração Centralizada**: Permite configurar centralmente os `HttpClients` com base em diferentes necessidades.
3. **Políticas de Resiliência**: Integração fácil com bibliotecas como Polly para aplicar políticas de retry, timeout e circuit breaker.
4. **Reuso de Código**: Reduz a duplicação de código ao permitir que você reutilize configurações em vários locais.

## Tipos de HttpClient no HttpClientFactory

### 1. Basic Usage

A maneira mais simples de usar o `HttpClientFactory` é injetar o `IHttpClientFactory` e usá-lo para criar instâncias de `HttpClient`.

**Exemplo:**

```csharp
public class MyService
{
    private readonly IHttpClientFactory _httpClientFactory;

    public MyService(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task<string> GetDataAsync()
    {
        var client = _httpClientFactory.CreateClient();
        var response = await client.GetStringAsync("https://api.example.com/data");
        return response;
    }
}
```

### 2. Named Clients

Os Named Clients permitem que você crie `HttpClients` com configurações específicas, associando-os a um nome.

**Configuração em `Program.cs`:**

```csharp
builder.Services.AddHttpClient("MyNamedClient", client =>
{
    client.BaseAddress = new Uri("https://api.example.com/");
    client.DefaultRequestHeaders.Add("User-Agent", "HttpClientFactory-Sample");
});
```

**Uso no Código:**

```csharp
public class MyService
{
    private readonly IHttpClientFactory _httpClientFactory;

    public MyService(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task<string> GetDataAsync()
    {
        var client = _httpClientFactory.CreateClient("MyNamedClient");
        var response = await client.GetStringAsync("/data");
        return response;
    }
}
```

### 3. Typed Clients

Os Typed Clients fornecem uma abordagem mais estruturada, permitindo definir um cliente fortemente tipado com métodos que encapsulam chamadas HTTP.

**Definindo um Typed Client:**

```csharp
public class MyTypedClient
{
    private readonly HttpClient _httpClient;

    public MyTypedClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _httpClient.BaseAddress = new Uri("https://api.example.com/");
    }

    public async Task<string> GetDataAsync()
    {
        var response = await _httpClient.GetStringAsync("/data");
        return response;
    }
}
```

**Registrando o Typed Client:**

```csharp
builder.Services.AddHttpClient<MyTypedClient>();
```

**Uso:**

```csharp
public class MyService
{
    private readonly MyTypedClient _typedClient;

    public MyService(MyTypedClient typedClient)
    {
        _typedClient = typedClient;
    }

    public async Task<string> GetDataAsync()
    {
        return await _typedClient.GetDataAsync();
    }
}
```

### 4. HttpClient com Handlers

É possível configurar handlers personalizados, como para logging ou autenticação, usando a extensão `AddHttpMessageHandler`.

**Exemplo de Configuração com Handler Customizado:**

```csharp
public class LoggingHandler : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        // Logica de logging aqui
        Console.WriteLine("Sending request...");
        var response = await base.SendAsync(request, cancellationToken);
        Console.WriteLine("Received response.");
        return response;
    }
}

builder.Services.AddHttpClient("MyClientWithHandler")
    .AddHttpMessageHandler<LoggingHandler>();
```

## Implementando HttpClientFactory em uma Aplicação ASP.NET Core

### Passo 1: Configurando o `HttpClientFactory`

1. Adicione o pacote `Microsoft.Extensions.Http` (já incluído por padrão em projetos ASP.NET Core).
2. Registre o serviço `HttpClient` no `Program.cs` usando `AddHttpClient`.

**Exemplo:**

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpClient();

var app = builder.Build();

app.MapGet("/", async (IHttpClientFactory httpClientFactory) =>
{
    var client = httpClientFactory.CreateClient();
    var response = await client.GetStringAsync("https://api.example.com/data");
    return response;
});

app.Run();
```

### Passo 2: Injetando e Usando o `HttpClientFactory`

Implemente o `HttpClientFactory` em um serviço como demonstrado anteriormente.

## Configurando Políticas de Resiliência com Polly

Polly é uma biblioteca de resiliência para .NET que permite definir políticas como retry, circuit breaker, timeout, e mais.

**Configuração com Polly:**

```csharp
using Polly;
using Polly.Extensions.Http;

builder.Services.AddHttpClient("MyPollyClient")
    .AddPolicyHandler(HttpPolicyExtensions
        .HandleTransientHttpError()
        .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))));
```

**Uso:**

```csharp
public class MyPollyService
{
    private readonly HttpClient _httpClient;

    public MyPollyService(IHttpClientFactory httpClientFactory)
    {
        _httpClient = httpClientFactory.CreateClient("MyPollyClient");
    }

    public async Task<string> GetDataAsync()
    {
        var response = await _httpClient.GetStringAsync("/data");
        return response;
    }
}
```

## Resumo

O `HttpClientFactory` é uma ferramenta essencial no .NET para gerenciar `HttpClient` de forma eficaz, reduzindo problemas de conectividade e melhorando a manutenção do código. Ele permite o uso de Named e Typed Clients, a integração com políticas de resiliência com Polly, e a configuração de Handlers personalizados, tornando as chamadas HTTP mais seguras e controladas.

Com o `HttpClientFactory`, você obtém um controle refinado sobre como as requisições HTTP são feitas, permitindo que suas aplicações sejam mais robustas, escaláveis e resilientes.