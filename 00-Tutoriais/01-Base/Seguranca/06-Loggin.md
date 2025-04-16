


# 📝 Implementação de Logging no ASP.NET Core 8

Este guia detalha como configurar **logging** em uma **Web API ASP.NET Core 8** usando o **provedor de log nativo** e o **Serilog** para logs estruturados. Os logs serão salvos no console, em arquivo e em banco de dados (SQL Server), integrando com projetos anteriores (ex.: autenticação JWT). O código é comentado e formatado para renderização no GitHub.

## 📘 Índice

1. O que é Logging?
2. Pacotes Necessários
3. Configuração do Logging Nativo
4. Configuração do Serilog
5. Salvando Logs no Banco de Dados
6. Usando Logs no AuthController
7. Boas Práticas e Segurança
8. Exemplo de Saída de Logs

---

## 1. ❓ O que é Logging?

**Logging** registra eventos, erros e informações da aplicação para monitoramento, depuração e auditoria. No ASP.NET Core, o logging é usado para:
- Rastrear requisições (ex.: logins na API).
- Diagnosticar erros (ex.: falhas no 2FA).
- Auditar ações (ex.: tentativas de acesso não autorizado).

Este guia configura logs para console, arquivo e banco de dados, com exemplos práticos.

---

## 2. 📦 Pacotes Necessários

Adicione os pacotes via NuGet para logging nativo, Serilog e integração com SQL Server:

```bash
dotnet add package Microsoft.Extensions.Logging
dotnet add package Serilog.AspNetCore
dotnet add package Serilog.Sinks.File
dotnet add package Serilog.Sinks.MSSqlServer
dotnet add package Microsoft.EntityFrameworkCore.SqlServer
```

**Explicação**:  
- `Microsoft.Extensions.Logging`: Provedor de log nativo do ASP.NET Core.  
- `Serilog.AspNetCore`: Integra o Serilog com ASP.NET Core.  
- `Serilog.Sinks.File`: Salva logs em arquivos.  
- `Serilog.Sinks.MSSqlServer`: Salva logs no SQL Server.  
- `Microsoft.EntityFrameworkCore.SqlServer`: Persiste dados (usado nos resumos anteriores).

---

## 3. ⚙️ Configuração do Logging Nativo

O ASP.NET Core inclui um sistema de logging básico que registra no console por padrão.

### `Program.cs`

Configure o logging nativo com níveis de log:

```csharp
var builder = WebApplication.CreateBuilder(args);

// Configura logging nativo
builder.Logging.ClearProviders(); // Remove provedores padrão
builder.Logging.AddConsole(); // Adiciona logs no console
builder.Logging.AddDebug(); // Adiciona logs no debugger (ex.: VS Code)
builder.Logging.SetMinimumLevel(LogLevel.Information); // Define nível mínimo

builder.Services.AddControllers();

var app = builder.Build();

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
```

**Explicação**:  
- `ClearProviders()`: Remove provedores padrão para personalização.  
- `AddConsole()`: Exibe logs no console.  
- `AddDebug()`: Exibe logs no debugger (útil em desenvolvimento).  
- `SetMinimumLevel`: Filtra logs abaixo de `Information` (ex.: `Debug` é ignorado).

### Exemplo de Uso

```csharp
[ApiController]
[Route("api/test")]
public class TestController : ControllerBase
{
    private readonly ILogger<TestController> _logger;

    public TestController(ILogger<TestController> logger)
    {
        _logger = logger;
    }

    [HttpGet]
    public IActionResult Get()
    {
        _logger.LogInformation("Requisição GET recebida em /api/test");
        try
        {
            throw new Exception("Erro simulado");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao processar requisição GET");
            return StatusCode(500, "Erro interno");
        }
    }
}
```

**Explicação**:  
- `ILogger<T>`: Injetado via construtor para logs específicos do controller.  
- `LogInformation`: Registra eventos normais.  
- `LogError`: Registra erros com detalhes da exceção.

---

## 4. 🚀 Configuração do Serilog

O **Serilog** oferece logs estruturados, mais poderosos que o provedor nativo, com suporte a múltiplos destinos (*sinks*).

### Pacotes

Já incluídos na seção 2 (`Serilog.AspNetCore`, `Serilog.Sinks.File`).

### `appsettings.json`

Configure o Serilog para console e arquivo:

```json
{
  "Serilog": {
    "Using": [ "Serilog.Sinks.Console", "Serilog.Sinks.File" ],
    "MinimumLevel": {
      "Default": "Information", // Nível padrão
      "Override": {
        "Microsoft": "Warning", // Reduz logs do framework
        "System": "Warning"
      }
    },
    "WriteTo": [
      { "Name": "Console" }, // Logs no console
      {
        "Name": "File",
        "Args": {
          "path": "Logs/app-.log", // Arquivo com data (ex.: app-20250416.log)
          "rollingInterval": "Day", // Novo arquivo por dia
          "retainedFileCountLimit": 7 // Mantém 7 dias
        }
      }
    ]
  }
}
```

**Explicação**:  
- `Using`: Lista os *sinks* usados.  
- `MinimumLevel`: Define `Information` como padrão, reduzindo logs verbosos do `Microsoft` e `System`.  
- `WriteTo`: Configura destinos (console e arquivo).  
- `File Args`: Cria arquivos diários em `Logs/` com até 7 dias de retenção.

### `Program.cs`

Integre o Serilog ao pipeline:

```csharp
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Configura Serilog
builder.Host.UseSerilog((context, configuration) =>
{
    configuration
        .ReadFrom.Configuration(context.Configuration) // Lê appsettings.json
        .Enrich.FromLogContext(); // Adiciona contexto (ex.: request ID)
});

builder.Services.AddControllers();

var app = builder.Build();

app.UseHttpsRedirection();
app.UseSerilogRequestLogging(); // Loga requisições HTTP
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
```

**Explicação**:  
- `UseSerilog`: Substitui o provedor nativo pelo Serilog.  
- `ReadFrom.Configuration`: Carrega configurações do `appsettings.json`.  
- `Enrich.FromLogContext`: Adiciona metadados aos logs.  
- `UseSerilogRequestLogging`: Registra detalhes de cada requisição (ex.: método, URL, status).

---

## 5. 💾 Salvando Logs no Banco de Dados

Configure o Serilog para salvar logs no SQL Server, útil para auditoria.

### Pacote

Já incluído (`Serilog.Sinks.MSSqlServer`).

### `appsettings.json`

Adicione o *sink* para SQL Server:

```json
{
  "Serilog": {
    "Using": [ "Serilog.Sinks.Console", "Serilog.Sinks.File", "Serilog.Sinks.MSSqlServer" ],
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft": "Warning",
        "System": "Warning"
      }
    },
    "WriteTo": [
      { "Name": "Console" },
      {
        "Name": "File",
        "Args": {
          "path": "Logs/app-.log",
          "rollingInterval": "Day",
          "retainedFileCountLimit": 7
        }
      },
      {
        "Name": "MSSqlServer",
        "Args": {
          "connectionString": "Server=localhost;Database=LogsDb;Trusted_Connection=True;",
          "schemaName": "dbo",
          "tableName": "Logs", // Tabela para logs
          "autoCreateSqlTable": true // Cria tabela automaticamente
        }
      }
    ]
  }
}
```

**Explicação**:  
- `MSSqlServer`: Configura o *sink* para SQL Server.  
- `connectionString`: Conecta ao banco `LogsDb`.  
- `tableName`: Define a tabela `Logs`.  
- `autoCreateSqlTable`: Cria a tabela se não existir.

### Estrutura da Tabela

A tabela `Logs` será criada com colunas como:
- `Id`: Chave primária.
- `Message`: Mensagem do log.
- `Level`: Nível (ex.: Information, Error).
- `TimeStamp`: Data e hora.
- `Exception`: Detalhes da exceção (se aplicável).

---

## 6. 🎮 Usando Logs no AuthController

Adapte o `AuthController` (dos resumos anteriores, ex.: 14/04/2025 e 16/04/2025) para usar Serilog.

```csharp
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

// Controlador com logging
[ApiController]
[Route("api/auth")]
[EnableCors("AllowFrontendAuth")] // Do resumo de CORS
public class AuthController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthController> _logger; // Logger nativo (compatível com Serilog)

    public AuthController(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IConfiguration configuration,
        ILogger<AuthController> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _configuration = configuration;
        _logger = logger;
    }

    // Login com 2FA (exemplo do resumo de 2FA)
    [HttpPost("login-2fa")]
    [AllowAnonymous]
    public async Task<IActionResult> LoginWithTwoFactor(TwoFactorLoginDTO dto)
    {
        _logger.LogInformation("Iniciando login 2FA para usuário {UserName}", dto.UserName);

        var user = await _userManager.FindByNameAsync(dto.UserName);
        if (user == null)
        {
            _logger.LogWarning("Tentativa de login com usuário inválido: {UserName}", dto.UserName);
            return Unauthorized(new { Message = "Credenciais inválidas." });
        }

        var passwordSignIn = await _signInManager.CheckPasswordSignInAsync(user, dto.Password, false);
        if (!passwordSignIn.Succeeded)
        {
            _logger.LogWarning("Senha incorreta para usuário {UserName}", dto.UserName);
            return Unauthorized(new { Message = "Credenciais inválidas." });
        }

        if (!await _userManager.GetTwoFactorEnabledAsync(user))
        {
            _logger.LogWarning("2FA não ativado para usuário {UserName}", dto.UserName);
            return BadRequest(new { Message = "2FA não está ativado." });
        }

        var isValidCode = await _userManager.VerifyTwoFactorTokenAsync(
            user,
            _userManager.Options.Tokens.AuthenticatorTokenProvider,
            dto.TwoFactorCode
        );
        if (!isValidCode)
        {
            _logger.LogWarning("Código 2FA inválido para usuário {UserName}", dto.UserName);
            return Unauthorized(new { Message = "Código 2FA inválido." });
        }

        try
        {
            var token = await GenerateJwtToken(user);
            _logger.LogInformation("Login 2FA bem-sucedido para usuário {UserName}", dto.UserName);
            return Ok(token);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao gerar JWT para usuário {UserName}", dto.UserName);
            return StatusCode(500, new { Message = "Erro interno." });
        }
    }

    // Reutilizado dos resumos anteriores
    private async Task<RespuestaAutenticacionDTO> GenerateJwtToken(ApplicationUser user)
    {
        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.UserName),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim("NomeCompleto", user.NomeCompleto),
            new Claim("Id", user.Id)
        };

        var userRoles = await _userManager.GetRolesAsync(user);
        claims.AddRange(userRoles.Select(role => new Claim(ClaimTypes.Role, role)));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expiry = DateTime.UtcNow.AddMinutes(30);

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: expiry,
            signingCredentials: creds
        );

        return new RespuestaAutenticacionDTO
        {
            Token = new JwtSecurityTokenHandler().WriteToken(token),
            Expiracion = expiry
        };
    }
}
```

**Explicação**:  
- `ILogger<AuthController>`: Injetado para logs específicos do controller.  
- `LogInformation`: Registra eventos normais (ex.: início e sucesso do login).  
- `LogWarning`: Registra falhas recuperáveis (ex.: usuário ou código inválido).  
- `LogError`: Registra erros graves com detalhes da exceção.  
- `{UserName}`: Adiciona contexto estruturado, visível no console, arquivo e banco.

---

## 7. 📌 Boas Práticas e Segurança

- **Níveis de Log**: Use níveis apropriados:
  - `Debug`: Detalhes para desenvolvimento.
  - `Information`: Eventos normais (ex.: login bem-sucedido).
  - `Warning`: Problemas recuperáveis (ex.: senha incorreta).
  - `Error`: Falhas graves (ex.: erro ao gerar JWT).
  - `Critical`: Falhas catastróficas (ex.: banco indisponível).  
- **Filtragem**: Reduza logs verbosos em produção com `MinimumLevel`:
  ```json
  "MinimumLevel": {
    "Default": "Warning",
    "Override": {
      "Microsoft": "Error",
      "System": "Error"
    }
  }
  ```
- **Estruturação**: Use propriedades estruturadas (ex.: `{UserName}`) para facilitar consultas:
  ```csharp
  _logger.LogInformation("Ação por {UserId} em {Endpoint}", user.Id, "/api/auth");
  ```
- **Segurança**: Não registre dados sensíveis (ex.: senhas, tokens JWT):
  ```csharp
  // Errado
  _logger.LogInformation("Senha fornecida: {Password}", dto.Password);
  // Correto
  _logger.LogInformation("Tentativa de login para {UserName}", dto.UserName);
  ```
- **Rotação de Arquivos**: Limite o tamanho e retenção de arquivos com `retainedFileCountLimit`.  
- **Banco de Dados**: Indexe a coluna `TimeStamp` na tabela `Logs` para consultas rápidas:
  ```sql
  CREATE INDEX IX_Logs_TimeStamp ON Logs(TimeStamp);
  ```
- **Monitoramento**: Considere *sinks* como ElasticSearch ou Application Insights para análise avançada em produção.  
- **Testes**: Verifique logs em testes de integração:
  ```csharp
  [Fact]
  public async Task Login2FA_LogsSuccess()
  {
      var loggerMock = new Mock<ILogger<AuthController>>();
      // Configurar teste
  }
  ```
- **Desempenho**: Evite logs excessivos em loops ou endpoints de alta frequência.

---

## 8. 📈 Exemplo de Saída de Logs

### Console

```plaintext
2025-04-16 10:09:32.123 [INF] Iniciando login 2FA para usuário "joao.silva"
2025-04-16 10:09:32.456 [INF] Login 2FA bem-sucedido para usuário "joao.silva"
2025-04-16 10:09:33.789 [WRN] Código 2FA inválido para usuário "maria.souza"
```

### Arquivo (`Logs/app-20250416.log`)

```plaintext
{"TimeStamp":"2025-04-16T10:09:32.123","Level":"Information","MessageTemplate":"Iniciando login 2FA para usuário {UserName}","Properties":{"UserName":"joao.silva"}}
{"TimeStamp":"2025-04-16T10:09:32.456","Level":"Information","MessageTemplate":"Login 2FA bem-sucedido para usuário {UserName}","Properties":{"UserName":"joao.silva"}}
{"TimeStamp":"2025-04-16T10:09:33.789","Level":"Warning","MessageTemplate":"Código 2FA inválido para usuário {UserName}","Properties":{"UserName":"maria.souza"}}
```

### Banco de Dados (`LogsDb.dbo.Logs`)

| Id | TimeStamp                | Level       | MessageTemplate                              | Properties                     |
|----|--------------------------|-------------|----------------------------------------------|--------------------------------|
| 1  | 2025-04-16 10:09:32.123 | Information | Iniciando login 2FA para usuário {UserName}   | {"UserName":"joao.silva"}      |
| 2  | 2025-04-16 10:09:32.456 | Information | Login 2FA bem-sucedido para usuário {UserName} | {"UserName":"joao.silva"}      |
| 3  | 2025-04-16 10:09:33.789 | Warning     | Código 2FA inválido para usuário {UserName}   | {"UserName":"maria.souza"}     |

---



---

### Integração com Resumos Anteriores

Este resumo é compatível com seus projetos anteriores:
- **Autenticação (14/04/2025)**: Adicione logs ao `AuthController` para registrar tentativas de login, refresh tokens, etc.
- **Envio de E-mails (16/04/2025)**: Logue envios de e-mails (ex.: sucesso ou falha no `EmailService`).
- **Login com Google (16/04/2025)**: Registre tentativas de login externo e erros no callback.
- **2FA (16/04/2025)**: O exemplo acima já integra logs no `login-2fa`.
- **CORS (16/04/2025)**: Logue erros de CORS (ex.: origens não permitidas) com o middleware de exceções.

Para integrar:
1. Adicione os pacotes Serilog ao projeto (`Serilog.AspNetCore`, etc.).
2. Configure o Serilog no `Program.cs` como mostrado.
3. Atualize o `appsettings.json` com as configurações de console, arquivo e banco.
4. Injete `ILogger<T>` nos controllers existentes (ex.: `AuthController`, `TwoFactorController`) e adicione logs como no exemplo.

