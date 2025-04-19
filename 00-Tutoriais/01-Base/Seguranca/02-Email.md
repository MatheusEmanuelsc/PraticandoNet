

# 📄 Tutorial Revisado: Implementando Refresh Tokens, Confirmação de E-mail, Recuperação de Senha, Roles/Claims e Integração com Unit of Work

## Resolvendo Suas Dúvidas

1. **Onde fazer as modificações no código?**
   - Cada seção do tutorial especifica o arquivo a ser criado ou modificado (ex.: `Services/AuthService.cs`, `Controllers/AuthController.cs`, `Program.cs`).
   - As modificações são feitas no projeto existente (que já tem ASP.NET Core Identity e JWT configurados, conforme o tutorial anterior).
   - Vou indicar claramente o caminho do arquivo e o que adicionar ou substituir.

2. **Pacotes devem ser reinstalados?**
   - **Não é necessário reinstalar** os pacotes já adicionados no tutorial anterior:
     - `Microsoft.AspNetCore.Identity.EntityFrameworkCore`
     - `Microsoft.AspNetCore.Authentication.JwtBearer`
     - `Microsoft.EntityFrameworkCore.SqlServer`
   - **Pacotes novos necessários** para este tutorial:
     - `System.Security.Cryptography.RandomNumberGenerator` (já incluído no .NET, não precisa instalar).
     - `System.Net.Mail` (para envio de e-mails via SMTP, também incluído no .NET).
     - Opcionalmente, se preferir usar um serviço como SendGrid, instale:
       ```bash
       dotnet add package SendGrid
       ```
   - Vou explicar quando e por que usar o pacote de e-mail.

3. **Pacote de e-mail deveria estar incluído?**
   - Sim, o tutorial inclui um serviço de e-mail (`EmailService`) para suportar **Confirmação de E-mail** e **Recuperação de Senha**.
   - O código usa a biblioteca padrão `System.Net.Mail` (SmtpClient) para simplicidade, mas você pode optar por serviços como SendGrid para maior confiabilidade.
   - Vou fornecer instruções claras para configurar o serviço de e-mail e alternativas.

4. **Por que o resumo anterior deixou você perdido?**
   - O tutorial anterior pode ter sido denso devido à quantidade de funcionalidades (Refresh Tokens, Confirmação de E-mail, etc.) e à integração com Unit of Work, sem indicar explicitamente onde cada trecho de código deve ser colocado.
   - Vou simplificar a estrutura, dividir em passos menores, e usar comentários para indicar **o que substituir** ou **adicionar** em arquivos existentes.

---

## Índice
1. [Introdução](#introdução)
2. [Pré-requisitos](#pré-requisitos)
3. [Pacotes Necessários](#pacotes-necessários)
4. [Implementando Funcionalidades Avançadas de Autenticação](#implementando-funcionalidades-avançadas-de-autenticação)
   - 4.1 [Refresh Tokens](#41-refresh-tokens)
   - 4.2 [Confirmação de E-mail](#42-confirmação-de-e-mail)
   - 4.3 [Recuperação de Senha](#43-recuperação-de-senha)
   - 4.4 [Roles e Claims Personalizados](#44-roles-e-claims-personalizados)
   - 4.5 [Proteção de Rotas com [Authorize]](#45-proteção-de-rotas-com-authorize)
5. [Integrando Identity com Unit of Work e Repository](#integrando-identity-com-unit-of-work-e-repository)
   - 5.1 [Estrutura Existente](#51-estrutura-existente)
   - 5.2 [Integrando IdentityDbContext](#52-integrando-identitydbcontext)
   - 5.3 [Integrando ApplicationDbContext](#53-integrando-applicationdbcontext)
6. [Boas Práticas: O que Fazer e o que Evitar](#boas-práticas-o-que-fazer-e-o-que-evitar)
7. [Conclusão](#conclusão)

---

## 1. Introdução

Este tutorial expande o projeto ASP.NET Core 8 configurado no tutorial anterior, que já tem **ASP.NET Core Identity**, **JWT**, **Unit of Work** e **Repository Pattern**. Vamos adicionar:
- **Refresh Tokens** para renovar tokens JWT.
- **Confirmação de E-mail** para validar contas.
- **Recuperação de Senha** via e-mail.
- **Roles e Claims Personalizados** para controle de acesso.
- **Proteção de Rotas** com `[Authorize]`.
- **Integração** do `IdentityDbContext` e `ApplicationDbContext` com Unit of Work.

O tutorial assume que você tem dois bancos de dados (`IdentityDb` e `DomainDb`) e que o projeto já está funcional com autenticação básica.

---

## 2. Pré-requisitos

- **Projeto ASP.NET Core 8** com:
  - ASP.NET Core Identity e JWT configurados (conforme tutorial anterior).
  - Repository Pattern e Unit of Work implementados.
- **SQL Server** com dois bancos: `IdentityDb` (para Identity) e `DomainDb` (para domínio).
- **Ferramentas**:
  - .NET 8 SDK.
  - Visual Studio, VS Code ou outro editor.
  - CLI do Entity Framework Core (`dotnet ef`).
- **Serviço de e-mail** (ex.: Gmail SMTP ou SendGrid) para Confirmação de E-mail e Recuperação de Senha.
- Conhecimento básico de C#, ASP.NET Core, e EF Core.

---

## 3. Pacotes Necessários

### Pacotes já instalados (do tutorial anterior)
- `Microsoft.AspNetCore.Identity.EntityFrameworkCore`: Para gerenciar usuários e roles.
- `Microsoft.AspNetCore.Authentication.JwtBearer`: Para autenticação JWT.
- `Microsoft.EntityFrameworkCore.SqlServer`: Para acesso ao SQL Server.

**Verifique se estão no arquivo `.csproj`**:
```xml
<PackageReference Include="Microsoft.AspNetCore.Identity.EntityFrameworkCore" Version="8.0.0" />
<PackageReference Include="Microsoft.AspNetCore.Authentication.JwtBearer" Version="8.0.0" />
<PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" Version="8.0.0" />
```

### Pacotes novos para este tutorial
- **Nenhum pacote adicional é estritamente necessário** para o código base, pois:
  - `System.Security.Cryptography.RandomNumberGenerator` (usado para gerar Refresh Tokens) está incluído no .NET.
  - `System.Net.Mail` (usado para envio de e-mails via SMTP) está incluído no .NET.
- **Opcional**: Para envio de e-mails mais robusto (ex.: SendGrid), instale:
  ```bash
  dotnet add package SendGrid
  ```
  - Use SendGrid se o SMTP (ex.: Gmail) for instável ou tiver limites de envio.

### Pacote de e-mail
- O tutorial usa `System.Net.Mail` (SmtpClient) por simplicidade, já incluído no .NET 8.
- **Por que incluí o e-mail?**
  - Confirmação de E-mail e Recuperação de Senha requerem envio de e-mails com links de validação.
  - O `EmailService` é essencial para essas funcionalidades.
- **Alternativa (SendGrid)**:
  - Se preferir SendGrid, substituirei o `EmailService` mais adiante com um exemplo usando SendGrid.

**Comando para verificar pacotes instalados**:
```bash
dotnet list package
```

---

## 4. Implementando Funcionalidades Avançadas de Autenticação

### 4.1 Refresh Tokens

Refresh Tokens permitem renovar tokens JWT sem exigir novo login, melhorando a experiência do usuário e a segurança.

#### Passo 1: Criar Modelo para Refresh Token
Crie um novo arquivo para a entidade `RefreshToken`.

**Arquivo**: `Models/RefreshToken.cs`
```csharp
using System;

namespace SeuProjeto.Models
{
    public class RefreshToken
    {
        public int Id { get; set; }
        public string Token { get; set; }
        public string UserId { get; set; }
        public DateTime ExpiryDate { get; set; }
        public bool IsRevoked { get; set; }
        public ApplicationUser User { get; set; }
    }
}
```

#### Passo 2: Atualizar o IdentityDbContext
Adicione o `DbSet` para `RefreshTokens` ao `IdentityDbContext`.

**Arquivo**: `Data/IdentityDbContext.cs`
**Modificação**: Adicione a propriedade `RefreshTokens`.

```csharp
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SeuProjeto.Models;

namespace SeuProjeto.Data
{
    public class IdentityDbContext : IdentityDbContext<ApplicationUser>
    {
        public DbSet<RefreshToken> RefreshTokens { get; set; } // Adicione esta linha

        public IdentityDbContext(DbContextOptions<IdentityDbContext> options) : base(options)
        {
        }
    }
}
```

#### Passo 3: Atualizar o AuthService
Substitua o conteúdo do `AuthService` para suportar Refresh Tokens.

**Arquivo**: `Services/AuthService.cs`
**Modificação**: Substitua o código existente pelo seguinte.

```csharp
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using SeuProjeto.Data;
using SeuProjeto.Models;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace SeuProjeto.Services
{
    public class AuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _configuration;
        private readonly IdentityDbContext _identityDbContext;

        public AuthService(UserManager<ApplicationUser> userManager, IConfiguration configuration, IdentityDbContext identityDbContext)
        {
            _userManager = userManager;
            _configuration = configuration;
            _identityDbContext = identityDbContext;
        }

        // Gera JWT e Refresh Token
        public async Task<(string JwtToken, string RefreshToken)> GenerateTokens(ApplicationUser user)
        {
            var jwtToken = await GenerateJwtToken(user);

            var refreshToken = new RefreshToken
            {
                Token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)),
                UserId = user.Id,
                ExpiryDate = DateTime.UtcNow.AddDays(7),
                IsRevoked = false
            };

            _identityDbContext.RefreshTokens.Add(refreshToken);
            await _identityDbContext.SaveChangesAsync();

            return (jwtToken, refreshToken.Token);
        }

        // Renova tokens usando um Refresh Token
        public async Task<(string JwtToken, string RefreshToken)?> RefreshToken(string refreshToken)
        {
            var token = await _identityDbContext.RefreshTokens
                .Include(rt => rt.User)
                .FirstOrDefaultAsync(rt => rt.Token == refreshToken && !rt.IsRevoked && rt.ExpiryDate > DateTime.UtcNow);

            if (token == null)
                return null;

            token.IsRevoked = true;
            _identityDbContext.RefreshTokens.Update(token);

            var newTokens = await GenerateTokens(token.User);
            await _identityDbContext.SaveChangesAsync();

            return newTokens;
        }

        // Gera apenas o JWT
        private async Task<string> GenerateJwtToken(ApplicationUser user)
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
}
```

**Notas**:
- **Adicione os `using`** necessários (ex.: `System.Security.Cryptography` para `RandomNumberGenerator`).
- O `IdentityDbContext` é injetado para gerenciar `RefreshTokens`.

#### Passo 4: Atualizar o AuthController
Modifique o endpoint de login e adicione um endpoint para renovar tokens.

**Arquivo**: `Controllers/AuthController.cs`
**Modificação**: Substitua o método `Login` e adicione o método `RefreshToken`.

```csharp
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SeuProjeto.DTOs;
using SeuProjeto.Services;

namespace SeuProjeto.Controllers
{
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

            var (jwtToken, refreshToken) = await _authService.GenerateTokens(user);
            return Ok(new { JwtToken = jwtToken, RefreshToken = refreshToken });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null || !await _userManager.CheckPasswordAsync(user, model.Password))
                return Unauthorized("Credenciais inválidas");

            var (jwtToken, refreshToken) = await _authService.GenerateTokens(user);
            return Ok(new { JwtToken = jwtToken, RefreshToken = refreshToken });
        }

        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken([FromBody] string refreshToken)
        {
            var tokens = await _authService.RefreshToken(refreshToken);
            if (tokens == null)
                return Unauthorized("Refresh token inválido ou expirado");

            return Ok(new { JwtToken = tokens.Value.JwtToken, RefreshToken = tokens.Value.RefreshToken });
        }
    }
}
```

#### Passo 5: Criar Migração
Crie e aplique uma migração para adicionar a tabela `RefreshTokens`.

**Comandos**:
```bash
dotnet ef migrations add AddRefreshTokens --context IdentityDbContext --output-dir Data/Migrations/Identity
dotnet ef database update --context IdentityDbContext
```

---

### 4.2 Confirmação de E-mail

A Confirmação de E-mail valida o endereço do usuário antes de permitir login.

#### Passo 1: Configurar Serviço de E-mail
Adicione configurações de e-mail no `appsettings.json`.

**Arquivo**: `appsettings.json`
**Modificação**: Adicione a seção `EmailSettings`.

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
  },
  "EmailSettings": {
    "SmtpServer": "smtp.gmail.com",
    "SmtpPort": 587,
    "SenderEmail": "seuemail@gmail.com",
    "SenderPassword": "sua-senha-de-app" // Use uma senha de aplicativo do Gmail
  }
}
```

**Nota sobre a senha**:
- Para Gmail, crie uma **senha de aplicativo** em `myaccount.google.com/security` (ative a autenticação de dois fatores).
- Alternativa: Use SendGrid (explicado na seção de alternativas).

#### Passo 2: Criar EmailService
Crie um serviço para enviar e-mails usando `System.Net.Mail`.

**Arquivo**: `Services/EmailService.cs`
**Ação**: Crie um novo arquivo.

```csharp
using System.Net.Mail;
using System.Net;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;

namespace SeuProjeto.Services
{
    public class EmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            var smtpServer = _configuration["EmailSettings:SmtpServer"];
            var smtpPort = int.Parse(_configuration["EmailSettings:SmtpPort"]);
            var senderEmail = _configuration["EmailSettings:SenderEmail"];
            var senderPassword = _configuration["EmailSettings:SenderPassword"];

            using var client = new SmtpClient(smtpServer, smtpPort)
            {
                Credentials = new NetworkCredential(senderEmail, senderPassword),
                EnableSsl = true
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(senderEmail),
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };
            mailMessage.To.Add(toEmail);

            await client.SendMailAsync(mailMessage);
        }
    }
}
```

#### Passo 3: Registrar EmailService
Registre o `EmailService` no contêiner de injeção de dependência.

**Arquivo**: `Program.cs`
**Modificação**: Adicione a linha abaixo após `builder.Services.AddScoped<AuthService>();`.

```csharp
builder.Services.AddScoped<EmailService>();
```

**Exemplo do trecho em `Program.cs`**:
```csharp
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<EmailService>(); // Adicione esta linha
```

#### Passo 4: Atualizar o AuthService
Adicione um método para enviar e-mails de confirmação.

**Arquivo**: `Services/AuthService.cs`
**Modificação**: Adicione o método `SendEmailConfirmationAsync` e atualize o construtor.

```csharp
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using SeuProjeto.Data;
using SeuProjeto.Models;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Web;

namespace SeuProjeto.Services
{
    public class AuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _configuration;
        private readonly IdentityDbContext _identityDbContext;
        private readonly EmailService _emailService; // Adicione

        public AuthService(UserManager<ApplicationUser> userManager, IConfiguration configuration, 
            IdentityDbContext identityDbContext, EmailService emailService) // Adicione EmailService
        {
            _userManager = userManager;
            _configuration = configuration;
            _identityDbContext = identityDbContext;
            _emailService = emailService;
        }

        // ... (métodos GenerateTokens, RefreshToken, GenerateJwtToken permanecem iguais)

        // Envia e-mail de confirmação
        public async Task SendEmailConfirmationAsync(ApplicationUser user)
        {
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var callbackUrl = $"https://sua-api.com/api/auth/confirm-email?userId={user.Id}&token={HttpUtility.UrlEncode(token)}";

            var body = $"<p>Por favor, confirme seu e-mail clicando <a href=\"{callbackUrl}\">aqui</a>.</p>";
            await _emailService.SendEmailAsync(user.Email, "Confirme seu E-mail", body);
        }
    }
}
```

#### Passo 5: Atualizar o AuthController
Modifique o endpoint de registro para enviar e-mail de confirmação e adicione um endpoint para confirmar e-mail.

**Arquivo**: `Controllers/AuthController.cs`
**Modificação**: Substitua o método `Register` e adicione o método `ConfirmEmail`.

```csharp
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SeuProjeto.DTOs;
using SeuProjeto.Services;

namespace SeuProjeto.Controllers
{
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

            await _authService.SendEmailConfirmationAsync(user);
            return Ok("Usuário registrado. Confirme seu e-mail para ativar a conta.");
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null || !await _userManager.CheckPasswordAsync(user, model.Password))
                return Unauthorized("Credenciais inválidas");

            if (!await _userManager.IsEmailConfirmedAsync(user))
                return BadRequest("E-mail não confirmado");

            var (jwtToken, refreshToken) = await _authService.GenerateTokens(user);
            return Ok(new { JwtToken = jwtToken, RefreshToken = refreshToken });
        }

        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken([FromBody] string refreshToken)
        {
            var tokens = await _authService.RefreshToken(refreshToken);
            if (tokens == null)
                return Unauthorized("Refresh token inválido ou expirado");

            return Ok(new { JwtToken = tokens.Value.JwtToken, RefreshToken = tokens.Value.RefreshToken });
        }

        [HttpGet("confirm-email")]
        public async Task<IActionResult> ConfirmEmail(string userId, string token)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return BadRequest("Usuário não encontrado");

            var result = await _userManager.ConfirmEmailAsync(user, token);
            if (!result.Succeeded)
                return BadRequest(result.Errors);

            return Ok("E-mail confirmado com sucesso");
        }
    }
}
```

#### Passo 6: Bloquear Login sem Confirmação
O método `Login` acima já inclui a verificação de e-mail confirmado.

---

### 4.3 Recuperação de Senha

Permite que usuários redefinam senhas via e-mail.

#### Passo 1: Atualizar o AuthService
Adicione um método para enviar e-mails de recuperação de senha.

**Arquivo**: `Services/AuthService.cs`
**Modificação**: Adicione o método `SendPasswordResetEmailAsync`.

```csharp
// Adicione ao final da classe AuthService
public async Task SendPasswordResetEmailAsync(string email)
{
    var user = await _userManager.FindByEmailAsync(email);
    if (user == null)
        return; // Não revela se o e-mail existe por segurança

    var token = await _userManager.GeneratePasswordResetTokenAsync(user);
    var callbackUrl = $"https://sua-api.com/api/auth/reset-password?userId={user.Id}&token={HttpUtility.UrlEncode(token)}";

    var body = $"<p>Redefina sua senha clicando <a href=\"{callbackUrl}\">aqui</a>.</p>";
    await _emailService.SendEmailAsync(user.Email, "Redefinir Senha", body);
}
```

#### Passo 2: Adicionar Endpoints ao AuthController
Adicione endpoints para iniciar e concluir a recuperação de senha.

**Arquivo**: `Controllers/AuthController.cs`
**Modificação**: Adicione os métodos `ForgotPassword` e `ResetPassword`.

```csharp
// Adicione ao final da classe AuthController
[HttpPost("forgot-password")]
public async Task<IActionResult> ForgottenPassword([FromBody] string email)
{
    await _authService.SendPasswordResetEmailAsync(email);
    return Ok("E-mail de recuperação enviado, se o endereço existir.");
}

[HttpPost("reset-password")]
public async Task<IActionResult> ResetPassword([FromQuery] string userId, [FromQuery] string token, [FromBody] ResetPasswordDto model)
{
    var user = await _userManager.FindByIdAsync(userId);
    if (user == null)
        return BadRequest("Usuário não encontrado");

    var result = await _userManager.ResetPasswordAsync(user, token, model.NewPassword);
    if (!result.Succeeded)
        return BadRequest(result.Errors);

    return Ok("Senha redefinida com sucesso");
}
```

#### Passo 3: Criar DTO para Redefinição
Crie um DTO para a redefinição de senha.

**Arquivo**: `DTOs/ResetPasswordDto.cs`
**Ação**: Crie um novo arquivo.

```csharp
namespace SeuProjeto.DTOs
{
    public class ResetPasswordDto
    {
        public string NewPassword { get; set; }
    }
}
```

---

### 4.4 Roles e Claims Personalizados

Roles e Claims permitem controle de acesso granular.

#### Passo 1: Criar Roles no Registro
Modifique o endpoint de registro para atribuir uma role padrão.

**Arquivo**: `Controllers/AuthController.cs`
**Modificação**: Atualize o método `Register`.

```csharp
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

    // Atribuir role padrão
    await _userManager.AddToRoleAsync(user, "User");

    await _authService.SendEmailConfirmationAsync(user);
    return Ok("Usuário registrado. Confirme seu e-mail para ativar a conta.");
}
```

#### Passo 2: Adicionar Roles ao JWT
Atualize o método `GenerateJwtToken` para incluir roles.

**Arquivo**: `Services/AuthService.cs`
**Modificação**: Substitua o método `GenerateJwtToken`.

```csharp
private async Task<string> GenerateJwtToken(ApplicationUser user)
{
    var claims = new List<Claim>
    {
        new Claim(JwtRegisteredClaimNames.Sub, user.Id),
        new Claim(JwtRegisteredClaimNames.Email, user.Email),
        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
    };

    // Adiciona roles como claims
    var roles = await _userManager.GetRolesAsync(user);
    claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

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
```

#### Passo 3: Criar Roles no Banco
Adicione código para criar roles iniciais.

**Arquivo**: `Program.cs`
**Modificação**: Adicione o código abaixo antes de `app.Run()`.

```csharp
using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var roles = new[] { "User", "Admin" };

    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
            await roleManager.CreateAsync(new IdentityRole(role));
    }
}
```

**Exemplo do trecho em `Program.cs`**:
```csharp
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// Criar roles iniciais
using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var roles = new[] { "User", "Admin" };

    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
            await roleManager.CreateAsync(new IdentityRole(role));
    }
}

app.Run();
```

---

### 4.5 Proteção de Rotas com [Authorize]

Crie um controlador com endpoints protegidos por roles.

**Arquivo**: `Controllers/SampleController.cs`
**Ação**: Crie um novo arquivo.

```csharp
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SeuProjeto.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SampleController : ControllerBase
    {
        [HttpGet("admin-only")]
        [Authorize(Roles = "Admin")]
        public IActionResult AdminOnly()
        {
            return Ok("Acesso exclusivo para Admins!");
        }

        [HttpGet("user-only")]
        [Authorize(Roles = "User")]
        public IActionResult UserOnly()
        {
            return Ok("Acesso para Usuários!");
        }
    }
}
```

---

## 5. Integrando Identity com Unit of Work e Repository

### 5.1 Estrutura Existente

Assumimos que você tem uma estrutura de **Unit of Work** e **Repository** como:

**Arquivo**: `Repositories/IRepository.cs`
```csharp
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SeuProjeto.Repositories
{
    public interface IRepository<TEntity> where TEntity : class
    {
        Task<TEntity> GetByIdAsync(int id);
        Task<IEnumerable<TEntity>> GetAllAsync();
        void Add(TEntity entity);
        void Update(TEntity entity);
        void Delete(TEntity entity);
    }
}
```

**Arquivo**: `Repositories/IUnitOfWork.cs`
```csharp
using System;
using System.Threading.Tasks;

namespace SeuProjeto.Repositories
{
    public interface IUnitOfWork : IDisposable
    {
        IRepository<TEntity> GetRepository<TEntity>() where TEntity : class;
        Task<int> SaveChangesAsync();
    }
}
```

### 5.2 Integrando IdentityDbContext

Crie um repositório para gerenciar `RefreshTokens`.

**Arquivo**: `Repositories/RefreshTokenRepository.cs`
**Ação**: Crie um novo arquivo.

```csharp
using Microsoft.EntityFrameworkCore;
using SeuProjeto.Data;
using SeuProjeto.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SeuProjeto.Repositories
{
    public class RefreshTokenRepository : IRepository<RefreshToken>
    {
        private readonly IdentityDbContext _context;

        public RefreshTokenRepository(IdentityDbContext context)
        {
            _context = context;
        }

        public async Task<RefreshToken> GetByIdAsync(int id)
        {
            return await _context.RefreshTokens.FindAsync(id);
        }

        public async Task<IEnumerable<RefreshToken>> GetAllAsync()
        {
            return await _context.RefreshTokens.ToListAsync();
        }

        public void Add(RefreshToken entity)
        {
            _context.RefreshTokens.Add(entity);
        }

        public void Update(RefreshToken entity)
        {
            _context.RefreshTokens.Update(entity);
        }

        public void Delete(RefreshToken entity)
        {
            _context.RefreshTokens.Remove(entity);
        }
    }
}
```

Crie ou atualize o `UnitOfWork` para incluir o repositório.

**Arquivo**: `Repositories/UnitOfWork.cs`
**Ação**: Crie ou substitua o arquivo.

```csharp
using SeuProjeto.Data;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SeuProjeto.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly IdentityDbContext _identityContext;
        private readonly ApplicationDbContext _domainContext;
        private readonly Dictionary<Type, object> _repositories = new();

        public UnitOfWork(IdentityDbContext identityContext, ApplicationDbContext domainContext)
        {
            _identityContext = identityContext;
            _domainContext = domainContext;
        }

        public IRepository<TEntity> GetRepository<TEntity>() where TEntity : class
        {
            var type = typeof(TEntity);
            if (!_repositories.ContainsKey(type))
            {
                if (type == typeof(RefreshToken))
                    _repositories[type] = new RefreshTokenRepository(_identityContext);
                // Adicione outros repositórios aqui
            }

            return (IRepository<TEntity>)_repositories[type];
        }

        public async Task<int> SaveChangesAsync()
        {
            var identityChanges = await _identityContext.SaveChangesAsync();
            var domainChanges = await _domainContext.SaveChangesAsync();
            return identityChanges + domainChanges;
        }

        public void Dispose()
        {
            _identityContext.Dispose();
            _domainContext.Dispose();
        }
    }
}
```

### 5.3 Integrando ApplicationDbContext

Crie um repositório para uma entidade de domínio (ex.: `Product`).

**Arquivo**: `Models/Product.cs`
**Ação**: Crie (se não existir).

```csharp
namespace SeuProjeto.Models
{
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
    }
}
```

**Arquivo**: `Repositories/ProductRepository.cs`
**Ação**: Crie um novo arquivo.

```csharp
using Microsoft.EntityFrameworkCore;
using SeuProjeto.Data;
using SeuProjeto.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SeuProjeto.Repositories
{
    public class ProductRepository : IRepository<Product>
    {
        private readonly ApplicationDbContext _context;

        public ProductRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Product> GetByIdAsync(int id)
        {
            return await _context.Products.FindAsync(id);
        }

        public async Task<IEnumerable<Product>> GetAllAsync()
        {
            return await _context.Products.ToListAsync();
        }

        public void Add(Product entity)
        {
            _context.Products.Add(entity);
        }

        public void Update(Product entity)
        {
            _context.Products.Update(entity);
        }

        public void Delete(Product entity)
        {
            _context.Products.Remove(entity);
        }
    }
}
```

Atualize o `UnitOfWork` para incluir o repositório de `Product`.

**Arquivo**: `Repositories/UnitOfWork.cs`
**Modificação**: Atualize o método `GetRepository`.

```csharp
public IRepository<TEntity> GetRepository<TEntity>() where TEntity : class
{
    var type = typeof(TEntity);
    if (!_repositories.ContainsKey(type))
    {
        if (type == typeof(RefreshToken))
            _repositories[type] = new RefreshTokenRepository(_identityContext);
        else if (type == typeof(Product))
            _repositories[type] = new ProductRepository(_domainContext);
    }

    return (IRepository<TEntity>)_repositories[type];
}
```

Registrar o `UnitOfWork`.

**Arquivo**: `Program.cs`
**Modificação**: Adicione a linha abaixo após `builder.Services.AddScoped<EmailService>();`.

```csharp
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
```

**Exemplo do trecho em `Program.cs`**:
```csharp
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<EmailService>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>(); // Adicione esta linha
```

---

## 6. Boas Práticas: O que Fazer e o que Evitar

### ✅ O que Fazer
- **Use HTTPS**: Proteja e-mails e tokens com conexões seguras.
- **Valide DTOs**: Adicione atributos como `[Required]` nos DTOs.
- **Teste E-mails**: Verifique o envio de e-mails em um ambiente de teste.
- **Armazene Refresh Tokens com Segurança**: Considere hashear os tokens no banco.
- **Organize Repositórios**: Mantenha repositórios separados para `IdentityDbContext` e `ApplicationDbContext`.

### ❌ O que Evitar
- **Não exponha tokens de confirmação**: Use os métodos do `UserManager` em vez de armazenar tokens manualmente.
- **Não ignore revogação de Refresh Tokens**: Revogue tokens antigos ao gerar novos.
- **Não misture contextos no Unit of Work**: Separe operações de `IdentityDbContext` e `ApplicationDbContext`.
- **Não envie e-mails sem log**: Registre falhas de envio para depuração.

---

## 7. Conclusão

Este tutorial adicionou:
- **Refresh Tokens** para renovação de JWT.
- **Confirmação de E-mail** e **Recuperação de Senha** com envio de e-mails.
- **Roles e Claims** para controle de acesso.
- **Proteção de Rotas** com `[Authorize]`.
- **Integração com Unit of Work** para gerenciar `IdentityDbContext` e `ApplicationDbContext`.

### Alternativa: Usando SendGrid
Se preferir SendGrid em vez de `SmtpClient`, instale o pacote:
```bash
dotnet add package SendGrid
```

Substitua o `EmailService` por:

**Arquivo**: `Services/EmailService.cs`
```csharp
using SendGrid;
using SendGrid.Helpers.Mail;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;

namespace SeuProjeto.Services
{
    public class EmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            var apiKey = _configuration["SendGrid:ApiKey"];
            var client = new SendGridClient(apiKey);
            var from = new EmailAddress("seuemail@exemplo.com", "Seu Projeto");
            var to = new EmailAddress(toEmail);
            var msg = MailHelper.CreateSingleEmail(from, to, subject, null, body);
            await client.SendEmailAsync(msg);
        }
    }
}
```

**appsettings.json**:
```json
{
  "SendGrid": {
    "ApiKey": "sua-chave-sendgrid"
  }
}
```

---

