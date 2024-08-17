### Limitação de Taxa (Rate Limiting) em .NET 8

**Sumário**

1. [O que é Limitação de Taxa](#o-que-e-limitacao-de-taxa)
2. [Vantagens de Implementar Rate Limiting](#vantagens-de-implementar-rate-limiting)
3. [Configurando Rate Limiting em .NET 8](#configurando-rate-limiting-em-net-8)
   - [Etapa 1: Ajuste no `Program`](#etapa-1-ajuste-no-program)
   - [Etapa 2: Usando Atributos nos Controladores](#etapa-2-usando-atributos-nos-controladores)
   - [Etapa 3: Definindo o Algoritmo](#etapa-3-definindo-o-algoritmo)
4. [Algoritmos de Limitação de Taxa](#algoritmos-de-limitacao-de-taxa)
   - [Limitador de Janela Fixa](#limitador-de-janela-fixa)
   - [Limitador de Janela Deslizante](#limitador-de-janela-deslizante)
   - [Limitador de Cesta de Token](#limitador-de-cesta-de-token)
   - [Limitador de Simultaneidade](#limitador-de-simultaneidade)
5. [Aplicando Rate Limiting Globalmente](#aplicando-rate-limiting-globalmente)
6. [Configurando Rate Limiting no `appsettings.json`](#configurando-rate-limiting-no-appsettingsjson)
7. [Exemplo Completo](#exemplo-completo)

---

### O que é Limitação de Taxa

A **Limitação de Taxa** (Rate Limiting) é uma técnica que restringe o número de requisições que podem ser feitas a uma aplicação em um determinado período. Isso é fundamental para:

- **Evitar sobrecarga** de servidores ou aplicativos.
- **Melhorar a segurança** e proteger contra ataques de negação de serviço (DDoS).
- **Reduzir custos** ao evitar o uso desnecessário de recursos.

### Vantagens de Implementar Rate Limiting

- **Proteção contra abusos:** Previne que usuários ou bots façam um número excessivo de requisições em um curto período.
- **Controle de fluxo:** Mantém o controle sobre o fluxo de tráfego, garantindo que os recursos da aplicação sejam utilizados de forma eficiente.
- **Estabilidade do sistema:** Ajudar a manter a estabilidade e disponibilidade da aplicação, mesmo sob alta carga.

### Configurando Rate Limiting em .NET 8

#### Etapa 1: Ajuste no `Program`

Para começar, adicione o serviço de limitação de taxa no `Program.cs`:

```csharp
builder.Services.AddRateLimited("nome", options =>
{
    // Definir o algoritmo de limitação de taxa
    // Exemplo:
    // options.AddFixedWindowLimiter("fixed", config => { ... });
});
```

Em seguida, adicione o middleware de limitação de taxa ao pipeline de requisições da aplicação, logo após `app.UseRouting();`:

```csharp
app.UseRateLimiter();
```

#### Etapa 2: Usando Atributos nos Controladores

Você pode usar atributos para habilitar ou desabilitar a limitação de taxa em controladores específicos:

```csharp
[EnableRateLimiting("nome")]
[DisableRateLimiting]
```

Também é possível definir um limitador de taxa global que afetará todos os endpoints.

#### Etapa 3: Definindo o Algoritmo

Existem quatro tipos principais de algoritmos de limitação de taxa:

1. **Limitador de Janela Fixa**
2. **Limitador de Janela Deslizante**
3. **Limitador de Cesta de Token**
4. **Limitador de Simultaneidade**

### Algoritmos de Limitação de Taxa

#### Limitador de Janela Fixa

O **Limitador de Janela Fixa** divide o tempo em janelas fixas e permite um número fixo de requisições dentro de cada janela. As requisições subsequentes são postergadas ou rejeitadas.

```csharp
builder.Services.AddRateLimited(rateLimitedOptions =>
{
    rateLimitedOptions.AddFixedWindowLimiter("fixed", options =>
    {
        options.PermitLimit = 3;
        options.Window = TimeSpan.FromSeconds(10);
        options.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        options.QueueLimit = 1;
    });

    rateLimitedOptions.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
});
```

**Exemplo:**
Permite 3 requisições a cada 10 segundos.

#### Limitador de Janela Deslizante

O **Limitador de Janela Deslizante** é semelhante ao de Janela Fixa, mas a janela de tempo é dividida em segmentos menores, e a janela desliza à medida que o tempo avança.

```csharp
builder.Services.AddRateLimited(rateLimitedOptions =>
{
    rateLimitedOptions.AddSlidingWindowLimiter("sliding", options =>
    {
        options.PermitLimit = 5;
        options.Window = TimeSpan.FromSeconds(10);
        options.SegmentsPerWindow = 2;
        options.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        options.QueueLimit = 2;
    });

    rateLimitedOptions.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
});
```

**Exemplo:**
Permite 5 requisições a cada 10 segundos, com a janela dividida em 2 segmentos.

#### Limitador de Cesta de Token

O **Limitador de Cesta de Token** utiliza um "balde" de tokens onde cada token representa uma requisição. Se o balde estiver vazio, a próxima requisição será rejeitada ou postergada.

```csharp
builder.Services.AddRateLimited(rateLimitedOptions =>
{
    rateLimitedOptions.AddTokenBucketLimiter("token", options =>
    {
        options.PermitLimit = 3;
        options.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        options.QueueLimit = 2;
        options.Window = TimeSpan.FromSeconds(5);
        options.TokensPerPeriod = 2;
        options.AutoReplenishment = true;
    });

    rateLimitedOptions.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
});
```

**Exemplo:**
Permite 3 requisições com 2 tokens por período de 5 segundos.

#### Limitador de Simultaneidade

O **Limitador de Simultaneidade** limita o número de requisições simultâneas que podem ser processadas pela aplicação.

```csharp
builder.Services.AddRateLimited(rateLimitedOptions =>
{
    rateLimitedOptions.AddConcurrencyLimiter("concurrency", options =>
    {
        options.PermitLimit = 5;
        options.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        options.QueueLimit = 1;
    });

    rateLimitedOptions.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
});
```

**Exemplo:**
Limita a 5 requisições simultâneas, com uma fila de espera de 1 requisição.

### Aplicando Rate Limiting Globalmente

Para aplicar a limitação de taxa globalmente, configure o `GlobalLimiter`:

```csharp
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.User.Identity?.Name ??
                         httpContext.Request.Headers.Host.ToString(),
            factory: partition => new FixedWindowRateLimiterOptions
            {
                AutoReplenishment = true,
                PermitLimit = 2,
                QueueLimit = 0,
                Window = TimeSpan.FromSeconds(10)
            }));
});
```

### Configurando Rate Limiting no `appsettings.json`

Você pode definir as opções de limitação de taxa no `appsettings.json` e depois vincular essas configurações a uma classe no código:

```json
"MyRateLimit": {
    "PermitLimit": 1,
    "Window": 5,
    "ReplenishmentPeriod": 1,
    "QueueLimit": 0,
    "SegmentsPerWindow": 4,
    "TokenLimit": 8,
    "TokenLimit2": 12,
    "TokensPerPeriod": 4,
    "AutoReplenishment": true
}
```

Crie uma classe para mapear essas configurações:

```csharp
namespace Curso.Api.RateLimitOptions
{
    public class MyRateLimitOptions
    {
        public const string MyRateLimit = "MyRateLimit";
        public int PermitLimit { get; set; } = 5;
        public int Window { get; set; } = 10;
        public int ReplenishmentPeriod { get; set; } = 2;
        public int QueueLimit { get; set; } = 2;
        public int SegmentsPerWindow { get; set; } = 8;
        public int TokenLimit { get; set; } = 10;
        public int TokenLimit2 { get; set; } = 20;
        public int TokensPerPeriod { get; set; } = 4;
        public bool AutoReplenishment { get; set; } = false;
    }
}
```

No `Program.cs`, configure o Rate Limiting utilizando essas opções:

```csharp
var myOptions = new MyRateLimitOptions();

builder.Configuration.GetSection(MyRateLimitOptions.MyRate

Limit).Bind(myOptions);

builder.Services.AddRateLimiter(rateLimitedOptions =>
{
    rateLimitedOptions.AddTokenBucketLimiter("token", options =>
    {
        options.PermitLimit = myOptions.PermitLimit;
        options.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        options.QueueLimit = myOptions.QueueLimit;
        options.Window = TimeSpan.FromSeconds(myOptions.Window);
        options.TokensPerPeriod = myOptions.TokensPerPeriod;
        options.AutoReplenishment = true;
    });

    rateLimitedOptions.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
});
```

### Exemplo Completo

Aqui está um exemplo completo de como implementar Rate Limiting em um projeto .NET 8:

```csharp
var builder = WebApplication.CreateBuilder(args);

// Configuração de Rate Limiting
builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("fixed", limiterOptions =>
    {
        limiterOptions.PermitLimit = 10;
        limiterOptions.Window = TimeSpan.FromSeconds(60);
        limiterOptions.QueueLimit = 2;
        limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
    });
});

var app = builder.Build();

app.UseRouting();

app.UseRateLimiter(); // Middleware de Rate Limiting

app.UseEndpoints(endpoints =>
{
    endpoints.MapGet("/", async context =>
    {
        await context.Response.WriteAsync("Rate Limiting Configurado!");
    }).RequireRateLimiting("fixed"); // Aplicando o limitador de taxa
});

app.Run();
```

Este exemplo aplica uma limitação de taxa com uma janela fixa de 60 segundos, permitindo até 10 requisições por minuto.

