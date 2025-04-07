

# 🧩 Configuração e Options no ASP.NET Core

## 📌 Índice

1. [Introdução](#introdução)
2. [Configuration](#configuration)
3. [Options Pattern](#options-pattern)
4. [IOptions](#ioptions)
5. [IOptionsSnapshot](#ioptionssnapshot)
6. [IOptionsMonitor](#ioptionsmonitor)
7. [Formas de Implementação](#formas-de-implementação)
8. [Comparativo e Cenários Recomendados](#comparativo-e-cenários-recomendados)
9. [Boas Práticas](#boas-práticas)
10. [Exemplos](#exemplos)

---

## 📖 Introdução

No ASP.NET Core, a configuração é baseada em um sistema de chave-valor que pode vir de arquivos (`appsettings.json`), variáveis de ambiente, argumentos de linha de comando, entre outros. Para organizar e acessar essas configurações de forma tipada e segura, utiliza-se o *Options Pattern*.

---

## 🛠 Configuration

### O que é?
É o sistema de chave-valor para carregar configurações da aplicação.

### Exemplos de fontes:
- `appsettings.json`
- `environment variables`
- `command line`
- `user-secrets`

### Acesso direto:
```csharp
var myValue = Configuration["Section:SubSection:Key"];
```

### Desvantagem:
- Acesso verboso e propenso a erro (string literal).

---

## 📦 Options Pattern

### O que é?
Permite mapear seções do `appsettings.json` para classes fortemente tipadas.

### Exemplo de configuração:
```json
"EmailSettings": {
  "Host": "smtp.example.com",
  "Port": 587
}
```

### Classe POCO:
```csharp
public class EmailSettings
{
    public string Host { get; set; }
    public int Port { get; set; }
}
```

---

## 🧷 IOptions

### Uso
- Para acessar configurações estáticas (fixas após o startup).
- Injetado como `IOptions<T>`

### Exemplo:
```csharp
public class EmailService
{
    private readonly EmailSettings _settings;

    public EmailService(IOptions<EmailSettings> options)
    {
        _settings = options.Value;
    }
}
```

### Cenário ideal:
- Configurações que não mudam durante o tempo de vida da aplicação.

---

## 🔁 IOptionsSnapshot

### Uso
- Para acessar configurações atualizadas a cada requisição (no escopo do request).
- Funciona somente em aplicações com escopo por requisição, como ASP.NET Core MVC/API.

### Injeção:
```csharp
public EmailService(IOptionsSnapshot<EmailSettings> snapshot)
{
    var settings = snapshot.Value;
}
```

### Cenário ideal:
- Configurações que mudam entre requisições e são reavaliadas sob demanda.
- Ex: comportamento customizável por tenant (multitenancy).

---

## 🧭 IOptionsMonitor

### Uso
- Para monitorar alterações em tempo real de configurações.
- Suporta evento `OnChange`.

### Exemplo:
```csharp
public EmailService(IOptionsMonitor<EmailSettings> monitor)
{
    monitor.OnChange(newSettings => {
        // reconfigura algo automaticamente
    });
}
```

### Cenário ideal:
- Background services.
- Serviços que precisam reagir dinamicamente a mudanças em tempo de execução.

---

## 🧩 Formas de Implementação

### 1. Direto no `Program.cs`
```csharp
builder.Services.Configure<EmailSettings>(
    builder.Configuration.GetSection("EmailSettings"));
```

### 2. Usando `AddOptions<T>()`
```csharp
builder.Services.AddOptions<EmailSettings>()
    .Bind(builder.Configuration.GetSection("EmailSettings"))
    .ValidateDataAnnotations(); // se usar atributos de validação
```

---

## ⚖️ Comparativo e Cenários Recomendados

| Interface           | Escopo        | Atualiza em runtime | Uso ideal                        |
|---------------------|---------------|----------------------|----------------------------------|
| `IOptions<T>`        | Singleton     | ❌                   | Configurações fixas              |
| `IOptionsSnapshot<T>`| Scoped        | ✅ (por request)     | Web APIs com dependência por request |
| `IOptionsMonitor<T>` | Singleton     | ✅ (tempo real)      | Background services, long-running |

---

## ✅ Boas Práticas

- Sempre valide com `ValidateDataAnnotations()` ou `.Validate(...)`.
- Evite `Configuration["Key"]` direto — use Options.
- Use `IOptionsMonitor` para reatividade.
- Para multitenancy, prefira `IOptionsSnapshot`.

---

## 💡 Exemplos

### Classe de configuração com validação
```csharp
public class EmailSettings
{
    [Required]
    public string Host { get; set; }

    [Range(1, 65535)]
    public int Port { get; set; }
}
```

### Injeção em controller
```csharp
[ApiController]
public class EmailController : ControllerBase
{
    private readonly EmailSettings _settings;

    public EmailController(IOptionsSnapshot<EmailSettings> options)
    {
        _settings = options.Value;
    }
}
```

### Uso com evento de mudança
```csharp
public class MonitorService
{
    public MonitorService(IOptionsMonitor<EmailSettings> monitor)
    {
        monitor.OnChange(settings =>
        {
            Console.WriteLine($"Config alterada: {settings.Host}:{settings.Port}");
        });
    }
}
```



