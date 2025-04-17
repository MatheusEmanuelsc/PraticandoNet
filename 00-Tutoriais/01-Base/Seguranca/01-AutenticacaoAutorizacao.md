Perfeito! Vou criar exatamente como pediu: um **tutorial passo a passo** para configurar o **ASP.NET Core Identity** em um projeto que **já tem Unit of Work e Repository**, e depois a **parte 2** com a implementação do **JWT**.

Vou organizar em `.md` (Markdown) com **índice**, **passos claros** e **explicações**.  
Começarei com a configuração do **Identity**, e em seguida, a **implementação do JWT**.

---

# 📄 ASP.NET Core 8 - Configurando Identity e JWT passo a passo

## Índice
1. [Introdução](#introdução)
2. [Configurar Identity no Projeto](#configurar-identity-no-projeto)
   - 2.1 [Adicionar Pacotes](#21-adicionar-pacotes)
   - 2.2 [Criar a ApplicationUser](#22-criar-a-applicationuser)
   - 2.3 [Criar o DbContext do Identity](#23-criar-o-dbcontext-do-identity)
   - 2.4 [Registrar o Identity no Program.cs](#24-registrar-o-identity-no-programcs)
3. [Configurar JWT Authentication](#configurar-jwt-authentication)
   - 3.1 [Adicionar Configurações ao appsettings.json](#31-adicionar-configurações-ao-appsettingsjson)
   - 3.2 [Criar DTOs de Login e Registro](#32-criar-dtos-de-login-e-registro)
   - 3.3 [Criar o AuthService](#33-criar-o-authservice)
   - 3.4 [Criar o AuthController](#34-criar-o-authcontroller)
4. [Conclusão](#conclusão)

---

# 1. Introdução

Este tutorial configura o ASP.NET Core Identity para gerenciar autenticação de usuários e depois integra a autenticação via **JWT** (Json Web Token) para APIs seguras.  
Este guia considera que você **já tem** implementado **Repository Pattern** e **Unit of Work** para sua camada de persistência.

---

# 2. Configurar Identity no Projeto

## 2.1 Adicionar Pacotes

Adicione os pacotes necessários ao seu projeto:

```bash
dotnet add package Microsoft.AspNetCore.Identity.EntityFrameworkCore
dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer
```

Se quiser suporte para Tokens mais avançados:

```bash
dotnet add package Microsoft.AspNetCore.Identity.UI
```

---

## 2.2 Criar a ApplicationUser

Crie uma classe para representar seu usuário no Identity:

```csharp
// Models/ApplicationUser.cs
using Microsoft.AspNetCore.Identity;

public class ApplicationUser : IdentityUser
{
    // Você pode adicionar propriedades extras aqui, como:
    public string FullName { get; set; }
}
```

---

## 2.3 Criar o DbContext do Identity

Crie um contexto exclusivo para Identity, separado do seu contexto de domínio:

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

---

## 2.4 Registrar o Identity no Program.cs

No `Program.cs`, registre o Identity, o DbContext e configure opções básicas:

```csharp
var builder = WebApplication.CreateBuilder(args);

// Configurar DbContext do Identity
builder.Services.AddDbContext<IdentityDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("IdentityConnection")));

// Configurar Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<IdentityDbContext>()
    .AddDefaultTokenProviders();

// Configurar senha, lockout, etc
builder.Services.Configure<IdentityOptions>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 6;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
});

// Configurar Cookies (opcional se usar JWT somente)
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/login";
    options.AccessDeniedPath = "/access-denied";
});

var app = builder.Build();
app.UseAuthentication();
app.UseAuthorization();
```

✅ Até aqui o Identity já está registrado e funcionando.

---

# 3. Configurar JWT Authentication

Agora vamos configurar JWT!

---

## 3.1 Adicionar Configurações ao appsettings.json

Abra o `appsettings.json` e adicione:

```json
"JwtSettings": {
  "Issuer": "SeuProjetoAPI",
  "Audience": "SeuProjetoAPI",
  "SecretKey": "sua_chave_secreta_muito_forte_123456"
}
```

---

## 3.2 Criar DTOs de Login e Registro

Crie DTOs para entrada de dados:

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

---

## 3.3 Criar o AuthService

Crie um serviço para encapsular a autenticação:

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
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IConfiguration _configuration;

    public AuthService(UserManager<ApplicationUser> userManager, IConfiguration configuration)
    {
        _userManager = userManager;
        _configuration = configuration;
    }

    public async Task<string> GenerateJwtToken(ApplicationUser user)
    {
        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtSettings:SecretKey"]));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _configuration["JwtSettings:Issuer"],
            audience: _configuration["JwtSettings:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(2),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
```

---

## 3.4 Criar o AuthController

Agora crie o Controller de autenticação:

```csharp
// Controllers/AuthController.cs
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly AuthService _authService;

    public AuthController(UserManager<ApplicationUser> userManager, AuthService authService)
    {
        _userManager = userManager;
        _authService = authService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterDto model)
    {
        var user = new ApplicationUser
        {
            UserName = model.Email,
            Email = model.Email,
            FullName = model.FullName
        };

        var result = await _userManager.CreateAsync(user, model.Password);

        if (!result.Succeeded)
            return BadRequest(result.Errors);

        var token = await _authService.GenerateJwtToken(user);
        return Ok(new { Token = token });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginDto model)
    {
        var user = await _userManager.FindByEmailAsync(model.Email);
        if (user == null)
            return Unauthorized();

        var result = await _userManager.CheckPasswordAsync(user, model.Password);
        if (!result)
            return Unauthorized();

        var token = await _authService.GenerateJwtToken(user);
        return Ok(new { Token = token });
    }
}
```

---

# 4. Conclusão

Agora você tem:

- ✅ Identity configurado no ASP.NET Core 8
- ✅ JWT implementado
- ✅ Registro e login de usuários via API
- ✅ Tokens seguros com validade de 2 horas
- ✅ Separação de responsabilidades (UserManager + AuthService)

---

# Notas Importantes:
- O **IdentityDbContext** é separado do seu **DbContext principal** (domínio).
- Configure suas **migrations** separadas também (`Add-Migration` e `Update-Database` usando o contexto correto).



builder.Services.AddScoped<AuthService>();
