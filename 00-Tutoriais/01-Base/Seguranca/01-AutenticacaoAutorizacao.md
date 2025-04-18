
# 📄 Tutorial Completo: Configurando ASP.NET Core Identity e JWT em um Projeto com Unit of Work e Repository

## Índice
1. [Introdução](#introdução)
2. [Pré-requisitos](#pré-requisitos)
3. [Configurando o ASP.NET Core Identity](#configurando-o-aspnet-core-identity)
   - 3.1 [Adicionar Pacotes](#31-adicionar-pacotes)
   - 3.2 [Criar a Classe ApplicationUser](#32-criar-a-classe-applicationuser)
   - 3.3 [Criar o IdentityDbContext](#33-criar-o-identitydbcontext)
   - 3.4 [Configurar o Identity no Program.cs](#34-configurar-o-identity-no-programcs)
4. [Implementando Autenticação JWT](#implementando-autenticação-jwt)
   - 4.1 [Configurar o appsettings.json](#41-configurar-o-appsettingsjson)
   - 4.2 [Criar DTOs para Registro e Login](#42-criar-dtos-para-registro-e-login)
   - 4.3 [Criar o AuthService](#43-criar-o-authservice)
   - 4.4 [Criar o AuthController](#44-criar-o-authcontroller)
5. [Gerenciando Migrações com Dois DbContexts](#gerenciando-migrações-com-dois-dbcontexts)
   - 5.1 [Configurar Strings de Conexão](#51-configurar-strings-de-conexão)
   - 5.2 [Criar e Aplicar Migrações](#52-criar-e-aplicar-migrações)
6. [Boas Práticas: O que Fazer e o que Evitar](#boas-práticas-o-que-fazer-e-o-que-evitar)
7. [Conclusão e Próximos Passos](#conclusão-e-próximos-passos)

---

## 1. Introdução

Este tutorial ensina como configurar o **ASP.NET Core Identity** para autenticação de usuários e implementar autenticação via **JWT** (JSON Web Token) em um projeto que já utiliza **Unit of Work** e **Repository Pattern**. O guia é voltado para projetos ASP.NET Core 8, com dois DbContexts (um para Identity e outro para o domínio), e inclui explicações detalhadas, código comentado e gestão de migrações.

---

## 2. Pré-requisitos

- **Projeto ASP.NET Core 8** configurado com Repository Pattern e Unit of Work.
- **Entity Framework Core** instalado para gerenciamento de banco de dados.
- **SQL Server** (ou outro banco compatível) configurado.
- **Conhecimento básico** de ASP.NET Core, injeção de dependência e APIs REST.

---

## 3. Configurando o ASP.NET Core Identity

### 3.1 Adicionar Pacotes

Adicione os pacotes necessários ao projeto via CLI:

```bash
dotnet add package Microsoft.AspNetCore.Identity.EntityFrameworkCore
dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer
```

Opcional (para suporte a UI ou tokens avançados):

```bash
dotnet add package Microsoft.AspNetCore.Identity.UI
```

### 3.2 Criar a Classe ApplicationUser

Crie uma classe que estende `IdentityUser` para representar o usuário:

```csharp
// Models/ApplicationUser.cs
using Microsoft.AspNetCore.Identity;

public class ApplicationUser : IdentityUser
{
    public string FullName { get; set; } // Propriedade personalizada
}
```

### 3.3 Criar o IdentityDbContext

Crie um DbContext para o Identity, separado do contexto do domínio:

```csharp
// Data/IdentityDbContext.cs
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

public class IdentityDbContext : IdentityDbContext<ApplicationUser>
{
    public IdentityDbContext(DbContextOptions<IdentityDbContext> options) : base(options)
    {
    }
}
```

### 3.4 Configurar o Identity no Program.cs

Registre o Identity, o DbContext e configure o pipeline de autenticação:

```csharp
// Program.cs
var builder = WebApplication.CreateBuilder(args);

// Registrar DbContext do Identity
builder.Services.AddDbContext<IdentityDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("IdentityConnection")));

// Registrar Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<IdentityDbContext>()
    .AddDefaultTokenProviders();

// Configurar regras de senha
builder.Services.Configure<IdentityOptions>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 6;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
});

// Registrar AuthService (correção adicionada)
builder.Services.AddScoped<AuthService>();

// Configurar autenticação JWT (detalhado na seção 4)
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
        ValidAudience = builder.Configuration["JwtSettings:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(builder.Configuration["JwtSettings:SecretKey"]))
    };
});

var app = builder.Build();

// Configurar pipeline
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers(); // Para APIs
app.Run();
```

---

## 4. Implementando Autenticação JWT

### 4.1 Configurar o appsettings.json

Adicione as configurações do JWT no `appsettings.json`:

```json
{
  "JwtSettings": {
    "Issuer": "SeuProjetoAPI",
    "Audience": "SeuProjetoAPI",
    "SecretKey": "sua_chave_secreta_muito_forte_123456"
  },
  "ConnectionStrings": {
    "IdentityConnection": "Server=localhost;Database=IdentityDb;Trusted_Connection=True;",
    "DomainConnection": "Server=localhost;Database=DomainDb;Trusted_Connection=True;"
  }
}
```

### 4.2 Criar DTOs para Registro e Login

Crie classes DTO para entrada de dados:

```csharp
// DTOs/RegisterDto.cs
public class RegisterDto
{
    public string Email { get; set; }
    public string Password { get; set; }
    public string FullName { get; set; }
}

// DTOs/LoginDto.cs
public class LoginDto
{
    public string Email { get; set; }
    public string Password { get; set; }
}
```

### 4.3 Criar o AuthService

O `AuthService` gera tokens JWT para usuários autenticados.

```csharp
// Services/AuthService.cs
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

public class AuthService
{
    private readonly UserManager<ApplicationUser> _userManager; // Gerencia operações de usuário
    private readonly IConfiguration _configuration; // Acessa configurações do JWT

    // Injeção de dependências
    public AuthService(UserManager<ApplicationUser> userManager, IConfiguration configuration)
    {
        _userManager = userManager;
        _configuration = configuration;
    }

    // Gera um token JWT para o usuário
    public async Task<string> GenerateJwtToken(ApplicationUser user)
    {
        // Define claims do token
        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id), // ID do usuário
            new Claim(JwtRegisteredClaimNames.Email, user.Email), // E-mail do usuário
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()) // ID único do token
            // Adicione mais claims (ex.: roles) se necessário
        };

        // Configura a chave de assinatura
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtSettings:SecretKey"]));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        // Cria o token JWT
        var token = new JwtSecurityToken(
            issuer: _configuration["JwtSettings:Issuer"],
            audience: _configuration["JwtSettings:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(2), // Expira em 2 horas
            signingCredentials: creds
        );

        // Retorna o token como string
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
```

**Explicação**:
- **Claims**: Incluem informações do usuário (ID, e-mail) e um identificador único (`Jti`).
- **Chave Secreta**: Usada para assinar o token, garantindo sua integridade.
- **Expiração**: Definida para 2 horas, exigindo reautenticação após esse período.

### 4.4 Criar o AuthController

O `AuthController` expõe endpoints para registro e login.

```csharp
// Controllers/AuthController.cs
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager; // Gerencia usuários
    private readonly AuthService _authService; // Gera tokens JWT

    // Injeção de dependências
    public AuthController(UserManager<ApplicationUser> userManager, AuthService authService)
    {
        _userManager = userManager;
        _authService = authService;
    }

    // POST: api/auth/register
    // Registra um novo usuário e retorna um token JWT
    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterDto model)
    {
        // Cria um novo usuário
        var user = new ApplicationUser
        {
            UserName = model.Email,
            Email = model.Email,
            FullName = model.FullName
        };

        // Salva o usuário no banco com a senha
        var result = await _userManager.CreateAsync(user, model.Password);

        // Verifica erros (ex.: e-mail duplicado, senha fraca)
        if (!result.Succeeded)
            return BadRequest(result.Errors);

        // Gera e retorna o token JWT
        var token = await _authService.GenerateJwtToken(user);
        return Ok(new { Token = token });
    }

    // POST: api/auth/login
    // Autentica um usuário e retorna um token JWT
    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginDto model)
    {
        // Busca o usuário pelo e-mail
        var user = await _userManager.FindByEmailAsync(model.Email);
        if (user == null)
            return Unauthorized(); // 401 se o usuário não existe

        // Verifica a senha
        var result = await _userManager.CheckPasswordAsync(user, model.Password);
        if (!result)
            return Unauthorized(); // 401 se a senha está errada

        // Gera e retorna o token JWT
        var token = await _authService.GenerateJwtToken(user);
        return Ok(new { Token = token });
    }
}
```

**Explicação**:
- **Registro**: Cria um usuário com o `UserManager` e retorna um token JWT se bem-sucedido.
- **Login**: Valida e-mail e senha, retornando um token JWT se corretos.
- **Códigos HTTP**: Usa `BadRequest` (400) para erros de validação e `Unauthorized` (401) para falhas de autenticação.

---

## 5. Gerenciando Migrações com Dois DbContexts

Como o projeto usa dois DbContexts (`IdentityDbContext` e o DbContext do domínio, ex.: `ApplicationDbContext`), as migrações devem ser gerenciadas separadamente.

### 5.1 Configurar Strings de Conexão

No `appsettings.json`, defina duas strings de conexão:

```json
{
  "ConnectionStrings": {
    "IdentityConnection": "Server=localhost;Database=IdentityDb;Trusted_Connection=True;",
    "DomainConnection": "Server=localhost;Database=DomainDb;Trusted_Connection=True;"
  }
}
```

### 5.2 Criar e Aplicar Migrações

#### Para o IdentityDbContext
1. Crie a migração:

```bash
dotnet ef migrations add InitialIdentityMigration --context IdentityDbContext --output-dir Data/Migrations/Identity
```

2. Aplique a migração:

```bash
dotnet ef database update --context IdentityDbContext
```

#### Para o ApplicationDbContext
1. Crie a migração:

```bash
dotnet ef migrations add InitialDomainMigration --context ApplicationDbContext --output-dir Data/Migrations/Domain
```

2. Aplique a migração:

```bash
dotnet ef database update --context ApplicationDbContext
```

**Explicação**:
- **`--context`**: Especifica qual DbContext usar.
- **`--output-dir`**: Organiza as migrações em pastas separadas.
- **Bancos Separados**: Cada contexto usa um banco diferente (`IdentityDb` e `DomainDb`) para evitar conflitos.

---

## 6. Boas Práticas: O que Fazer e o que Evitar

### ✅ O que Fazer
- **Use HTTPS**: Proteja senhas e tokens com conexões seguras.
- **Valide DTOs**: Adicione validações (ex.: `[Required]`) nos DTOs para entradas seguras.
- **Chave Secreta Forte**: Use uma `SecretKey` longa e armazene-a em variáveis de ambiente ou serviços como Azure Key Vault.
- **Organize Migrações**: Separe migrações em pastas distintas para cada DbContext.
- **Teste Localmente**: Valide migrações e endpoints em um ambiente local antes de produção.

### ❌ O que Evitar
- **Não exponha a SecretKey**: Nunca deixe a chave secreta no código ou no repositório.
- **Não use o mesmo banco para os dois contextos**: Isso causa conflitos nas tabelas.
- **Não ignore erros de migração**: Leia os erros do `dotnet ef` para corrigir problemas.
- **Não retorne erros detalhados no login**: Evite mensagens como "E-mail não encontrado" para não expor informações sensíveis.
- **Não desative validações de senha**: Mantenha as regras do Identity para senhas seguras.

---

## 7. Conclusão e Próximos Passos

Este tutorial configurou o **ASP.NET Core Identity** e a autenticação **JWT** em um projeto com **Unit of Work** e **Repository Pattern**. Você agora tem:
- Um sistema de autenticação com registro e login via API.
- Tokens JWT com expiração de 2 horas.
- Dois DbContexts gerenciados separadamente com migrações organizadas.
- Boas práticas para segurança e organização.

### Próximos Passos
Se desejar, posso criar tutoriais adicionais para:
1. **Implementar**:
   - Refresh Tokens
   - Confirmação de e-mail
   - Recuperação de senha
   - Roles e Claims personalizados
   - Proteção de rotas com `[Authorize]`
2. **Integrar com Unit of Work**:
   - Mostrar como conectar o `IdentityDbContext` e o `ApplicationDbContext` ao seu padrão UoW/Repository.
3. **Exemplos Práticos**:
   - Um endpoint protegido com validação de roles ou claims.

