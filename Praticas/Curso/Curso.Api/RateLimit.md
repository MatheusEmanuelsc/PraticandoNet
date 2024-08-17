### Limita��o de Taxa (Rate Limiting) em .NET 8

**Sum�rio**

1. [O que � Limita��o de Taxa](#o-que-e-limitacao-de-taxa)
2. [Vantagens de Implementar Rate Limiting](#vantagens-de-implementar-rate-limiting)
3. [Configurando Rate Limiting em .NET 8](#configurando-rate-limiting-em-net-8)
   - [Etapa 1: Ajuste no `Program`](#etapa-1-ajuste-no-program)
   - [Etapa 2: Usando Atributos nos Controladores](#etapa-2-usando-atributos-nos-controladores)
   - [Etapa 3: Definindo o Algoritmo](#etapa-3-definindo-o-algoritmo)
4. [Algoritmos de Limita��o de Taxa](#algoritmos-de-limitacao-de-taxa)
   - [Limitador de Janela Fixa](#limitador-de-janela-fixa)
   - [Limitador de Janela Deslizante](#limitador-de-janela-deslizante)
   - [Limitador de Cesta de Token](#limitador-de-cesta-de-token)
   - [Limitador de Simultaneidade](#limitador-de-simultaneidade)
5. [Aplicando Rate Limiting Globalmente](#aplicando-rate-limiting-globalmente)
6. [Configurando Rate Limiting no `appsettings.json`](#configurando-rate-limiting-no-appsettingsjson)
7. [Exemplo Completo](#exemplo-completo)

---

### O que � Limita��o de Taxa

A **Limita��o de Taxa** (Rate Limiting) � uma t�cnica que restringe o n�mero de requisi��es que podem ser feitas a uma aplica��o em um determinado per�odo. Isso � fundamental para:

- **Evitar sobrecarga** de servidores ou aplicativos.
- **Melhorar a seguran�a** e proteger contra ataques de nega��o de servi�o (DDoS).
- **Reduzir custos** ao evitar o uso desnecess�rio de recursos.

### Vantagens de Implementar Rate Limiting

- **Prote��o contra abusos:** Previne que usu�rios ou bots fa�am um n�mero excessivo de requisi��es em um curto per�odo.
- **Controle de fluxo:** Mant�m o controle sobre o fluxo de tr�fego, garantindo que os recursos da aplica��o sejam utilizados de forma eficiente.
- **Estabilidade do sistema:** Ajudar a manter a estabilidade e disponibilidade da aplica��o, mesmo sob alta carga.

### Configurando Rate Limiting em .NET 8

#### Etapa 1: Ajuste no `Program`

Para come�ar, adicione o servi�o de limita��o de taxa no `Program.cs`:

```csharp
builder.Services.AddRateLimited("nome", options =>
{
    // Definir o algoritmo de limita��o de taxa
    // Exemplo:
    // options.AddFixedWindowLimiter("fixed", config => { ... });
});
```

Em seguida, adicione o middleware de limita��o de taxa ao pipeline de requisi��es da aplica��o, logo ap�s `app.UseRouting();`:

```csharp
app.UseRateLimiter();
```

#### Etapa 2: Usando Atributos nos Controladores

Voc� pode usar atributos para habilitar ou desabilitar a limita��o de taxa em controladores espec�ficos:

```csharp
[EnableRateLimiting("nome")]
[DisableRateLimiting]
```

Tamb�m � poss�vel definir um limitador de taxa global que afetar� todos os endpoints.

#### Etapa 3: Definindo o Algoritmo

Existem quatro tipos principais de algoritmos de limita��o de taxa:

1. **Limitador de Janela Fixa**
2. **Limitador de Janela Deslizante**
3. **Limitador de Cesta de Token**
4. **Limitador de Simultaneidade**

### Algoritmos de Limita��o de Taxa

#### Limitador de Janela Fixa

O **Limitador de Janela Fixa** divide o tempo em janelas fixas e permite um n�mero fixo de requisi��es dentro de cada janela. As requisi��es subsequentes s�o postergadas ou rejeitadas.

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
Permite 3 requisi��es a cada 10 segundos.

#### Limitador de Janela Deslizante

O **Limitador de Janela Deslizante** � semelhante ao de Janela Fixa, mas a janela de tempo � dividida em segmentos menores, e a janela desliza � medida que o tempo avan�a.

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
Permite 5 requisi��es a cada 10 segundos, com a janela dividida em 2 segmentos.

#### Limitador de Cesta de Token

O **Limitador de Cesta de Token** utiliza um "balde" de tokens onde cada token representa uma requisi��o. Se o balde estiver vazio, a pr�xima requisi��o ser� rejeitada ou postergada.

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
Permite 3 requisi��es com 2 tokens por per�odo de 5 segundos.

#### Limitador de Simultaneidade

O **Limitador de Simultaneidade** limita o n�mero de requisi��es simult�neas que podem ser processadas pela aplica��o.

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
Limita a 5 requisi��es simult�neas, com uma fila de espera de 1 requisi��o.

### Aplicando Rate Limiting Globalmente

Para aplicar a limita��o de taxa globalmente, configure o `GlobalLimiter`:

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

Voc� pode definir as op��es de limita��o de taxa no `appsettings.json` e depois vincular essas configura��es a uma classe no c�digo:

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

Crie uma classe para mapear essas configura��es:

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

No `Program.cs`, configure o Rate Limiting utilizando essas op��es:

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

Aqui est� um exemplo completo de como implementar Rate Limiting em um projeto .NET 8:

```csharp
var builder = WebApplication.CreateBuilder(args);

// Configura��o de Rate Limiting
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

Este exemplo aplica uma limita��o de taxa com uma janela fixa de 60 segundos, permitindo at� 10 requisi��es por minuto.

