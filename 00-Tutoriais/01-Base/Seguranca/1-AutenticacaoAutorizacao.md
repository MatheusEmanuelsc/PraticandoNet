


```markdown
# 🔐 Autenticação e Autorização com ASP.NET Core 8 (Identity + JWT)

Este guia implementa um sistema robusto de autenticação e autorização em **ASP.NET Core 8** usando **Identity**, **JWT**, **Refresh Tokens**, **Recuperação de Senha** e **Autorização com Roles e Claims**. A confirmação de e-mail foi removida para implementação futura. O código é comentado detalhadamente, seguindo boas práticas de segurança.

## 📘 Índice

1. Pacotes Necessários
2. Configuração do Identity
3. Configuração do JWT
4. Models e DTOs
5. AuthController
6. Autorização com Roles e Claims
7. Protegendo Endpoints
8. Boas Práticas e Segurança
9. Tabela de Endpoints

---

## 1. 📦 Pacotes Necessários

Adicione os pacotes necessários via NuGet:

```bash
dotnet add package Microsoft.AspNetCore.Identity.EntityFrameworkCore
dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer
dotnet add package Microsoft.EntityFrameworkCore.SqlServer
```

---

## 2. ⚙️ Configuração do Identity

No `Program.cs`, configuramos o **Identity** para gerenciar usuários e roles.

```csharp
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = true; // Senha deve conter pelo menos um dígito
    options.Password.RequiredLength = 8; // Senha deve ter no mínimo 8 caracteres
})
.AddEntityFrameworkStores<ApplicationDbContext>() // Configura o Identity para usar o Entity Framework
.AddDefaultTokenProviders(); // Habilita provedores para geração de tokens (ex.: redefinição de senha)
```

**Explicação de `AddDefaultTokenProviders`**:  
Registra provedores padrão do ASP.NET Identity para gerar tokens seguros, como os usados na redefinição de senha (ex.: `GeneratePasswordResetTokenAsync`).

---

## 3. 🔑 Configuração do JWT

### `appsettings.json`:

```json
{
  "Jwt": {
    "Key": "sua-chave-secreta-de-32-caracteres-ou-mais", // Chave secreta para assinar tokens JWT
    "Issuer": "MinhaApi", // Identifica o emissor do token
    "Audience": "ClientesDaMinhaApi" // Identifica os destinatários autorizados
  }
}
```

### `Program.cs`:

```csharp
var configuration = builder.Configuration;

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme; // Define JWT como esquema padrão
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme; // Define JWT para desafios
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true, // Valida o emissor
        ValidIssuer = configuration["Jwt:Issuer"],
        ValidateAudience = true, // Valida a audiência
        ValidAudience = configuration["Jwt:Audience"],
        ValidateIssuerSigningKey = true, // Valida a chave de assinatura
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Key"])),
        ValidateLifetime = true, // Verifica expiração
        ClockSkew = TimeSpan.Zero // Sem tolerância para expiração
    };
});

builder.Services.AddAuthorization(); // Habilita serviços de autorização
```

---

## 4. 👤 Models e DTOs

### Models

#### `ApplicationUser`:

```csharp
public class ApplicationUser : IdentityUser
{
    public string NomeCompleto { get; set; } = string.Empty; // Campo personalizado para nome completo
}
```

#### `RefreshToken`:

```csharp
public class RefreshToken
{
    public int Id { get; set; } // Identificador único
    public string Token { get; set; } = null!; // Valor do refresh token
    public string UserId { get; set; } = null!; // ID do usuário
    public string JwtId { get; set; } = null!; // ID do JWT associado
    public DateTime AddedDate { get; set; } // Data de criação
    public DateTime Expiration { get; set; } // Data de expiração
    public bool IsUsed { get; set; } // Indica se foi usado
    public bool IsRevoked { get; set; } // Indica se foi revogado
}
```

### DTOs

#### `RegisterDTO`:

```csharp
public class RegisterDTO
{
    [Required]
    public string UserName { get; set; } = null!; // Nome de usuário
    [Required, EmailAddress]
    public string Email { get; set; } = null!; // E-mail válido
    [Required, MinLength(8)]
    public string Password { get; set; } = null!; // Senha com mínimo de 8 caracteres
    [Required]
    public string NomeCompleto { get; set; } = null!; // Nome completo
}
```

#### `LoginDTO`:

```csharp
public class LoginDTO
{
    [Required]
    public string UserName { get; set; } = null!; // Nome de usuário
    [Required]
    public string Password { get; set; } = null!; // Senha
}
```

#### `RefreshTokenDTO`:

```csharp
public class RefreshTokenDTO
{
    [Required]
    public string RefreshToken { get; set; } = null!; // Refresh token
}
```

#### `ResetPasswordDTO`:

```csharp
public class ResetPasswordDTO
{
    [Required, EmailAddress]
    public string Email { get; set; } = null!; // E-mail
    [Required]
    public string Token { get; set; } = null!; // Token de redefinição
    [Required, MinLength(8)]
    public string NovaSenha { get; set; } = null!; // Nova senha
}
```

#### `RespuestaAutenticacionDTO`:

```csharp
public class RespuestaAutenticacionDTO
{
    public string Token { get; set; } = null!; // JWT gerado
    public string RefreshToken { get; set; } = null!; // Refresh token
    public DateTime Expiracion { get; set; } // Expiração do JWT
}
```

---

## 5. 🎮 AuthController

Implementa endpoints para autenticação e gerenciamento de usuários.

```csharp
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

// Controlador para autenticação e gerenciamento
[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager; // Gerencia usuários
    private readonly RoleManager<IdentityRole> _roleManager; // Gerencia roles
    private readonly IConfiguration _configuration; // Acessa configurações
    private readonly ApplicationDbContext _context; // Contexto do banco

    public AuthController(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        IConfiguration configuration,
        ApplicationDbContext context)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _configuration = configuration;
        _context = context;
    }

    // Registra um novo usuário
    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register(RegisterDTO dto)
    {
        // Cria usuário com dados fornecidos
        var user = new ApplicationUser
        {
            UserName = dto.UserName,
            Email = dto.Email,
            NomeCompleto = dto.NomeCompleto
        };

        // Salva usuário no banco
        var result = await _userManager.CreateAsync(user, dto.Password);
        if (!result.Succeeded)
            return BadRequest(new { Errors = result.Errors });

        return Ok(new { Message = "Registro bem-sucedido." });
    }

    // Autentica o usuário e retorna JWT e refresh token
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login(LoginDTO dto)
    {
        // Busca usuário
        var user = await _userManager.FindByNameAsync(dto.UserName);
        // Verifica credenciais
        if (user == null || !await _userManager.CheckPasswordAsync(user, dto.Password))
            return Unauthorized(new { Message = "Credenciais inválidas." });

        // Gera tokens
        var tokenResult = await GenerateJwtToken(user);
        return Ok(tokenResult);
    }

    // Renova o JWT usando refresh token
    [HttpPost("refresh-token")]
    [AllowAnonymous]
    public async Task<IActionResult> RefreshToken(RefreshTokenDTO dto)
    {
        // Busca refresh token
        var refreshToken = await _context.RefreshTokens.FirstOrDefaultAsync(rt => rt.Token == dto.RefreshToken);
        // Valida token
        if (refreshToken == null || refreshToken.IsRevoked || refreshToken.IsUsed || refreshToken.Expiration < DateTime.UtcNow)
            return BadRequest(new { Message = "Refresh token inválido." });

        // Busca usuário
        var user = await _userManager.FindByIdAsync(refreshToken.UserId);
        if (user == null)
            return NotFound(new { Message = "Usuário não encontrado." });

        // Marca token como usado
        refreshToken.IsUsed = true;
        _context.Update(refreshToken);
        await _context.SaveChangesAsync();

        // Gera novo JWT
        var jwtResult = await GenerateJwtToken(user);
        return Ok(jwtResult);
    }

    // Inicia recuperação de senha
    [HttpPost("forgot-password")]
    [AllowAnonymous]
    public async Task<IActionResult> ForgotPassword([FromBody] string email)
    {
        // Busca usuário
        var user = await _userManager.FindByEmailAsync(email);
        // Resposta genérica para segurança
        if (user == null)
            return Ok(new { Message = "Se o e-mail estiver cadastrado, um link será enviado." });

        // Gera token de redefinição
        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        // Cria link
        var resetLink = Url.Action("ResetPassword", "Auth", new { userId = user.Id, token }, Request.Scheme);
        // TODO: Enviar e-mail com resetLink
        Console.WriteLine($"Link de redefinição: {resetLink}");

        return Ok(new { Message = "Link de recuperação enviado." });
    }

    // Redefine a senha
    [HttpPost("reset-password")]
    [AllowAnonymous]
    public async Task<IActionResult> ResetPassword(ResetPasswordDTO dto)
    {
        // Busca usuário
        var user = await _userManager.FindByEmailAsync(dto.Email);
        if (user == null)
            return BadRequest(new { Message = "Usuário não encontrado." });

        // Redefine senha
        var result = await _userManager.ResetPasswordAsync(user, dto.Token, dto.NovaSenha);
        if (result.Succeeded)
            return Ok(new { Message = "Senha redefinida com sucesso!" });
        return BadRequest(new { Errors = result.Errors });
    }

    // Atribui role a usuário
    [HttpPost("assign-role")]
    [Authorize(Policy = "RequireAdminRole")]
    public async Task<IActionResult> AssignRole(string email, string roleName)
    {
        // Busca usuário
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
            return NotFound(new { Message = "Usuário não encontrado." });

        // Cria role se necessário
        if (!await _roleManager.RoleExistsAsync(roleName))
            await _roleManager.CreateAsync(new IdentityRole(roleName));

        // Atribui role
        var result = await _userManager.AddToRoleAsync(user, roleName);
        if (result.Succeeded)
            return Ok(new { Message = $"Usuário {email} adicionado à role {roleName}." });
        return BadRequest(new { Errors = result.Errors });
    }

    // Remove role de usuário
    [HttpPost("remove-role")]
    [Authorize(Policy = "RequireAdminRole")]
    public async Task<IActionResult> RemoveRole(string email, string roleName)
    {
        // Busca usuário
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
            return NotFound(new { Message = "Usuário não encontrado." });

        // Remove role
        var result = await _userManager.RemoveFromRoleAsync(user, roleName);
        if (result.Succeeded)
            return Ok(new { Message = $"Usuário {email} removido da role {roleName}." });
        return BadRequest(new { Errors = result.Errors });
    }

    // Gera JWT e refresh token
    private async Task<RespuestaAutenticacionDTO> GenerateJwtToken(ApplicationUser user)
    {
        // Define claims
        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.UserName),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim("NomeCompleto", user.NomeCompleto),
            new Claim("Id", user.Id)
        };

        // Adiciona roles
        var userRoles = await _userManager.GetRolesAsync(user);
        claims.AddRange(userRoles.Select(role => new Claim(ClaimTypes.Role, role)));

        // Adiciona claims personalizadas
        var userClaims = await _userManager.GetClaimsAsync(user);
        claims.AddRange(userClaims);

        // Configura assinatura
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expiry = DateTime.UtcNow.AddMinutes(30);

        // Cria JWT
        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: expiry,
            signingCredentials: creds);

        // Gera e salva refresh token
        var refreshToken = GenerateRefreshToken();
        refreshToken.UserId = user.Id;
        refreshToken.JwtId = token.Id;
        refreshToken.Expiration = DateTime.UtcNow.AddDays(7);
        await _context.RefreshTokens.AddAsync(refreshToken);
        await _context.SaveChangesAsync();

        // Retorna tokens
        return new RespuestaAutenticacionDTO
        {
            Token = new JwtSecurityTokenHandler().WriteToken(token),
            RefreshToken = refreshToken.Token,
            Expiracion = expiry
        };
    }

    // Gera refresh token seguro
    private RefreshToken GenerateRefreshToken()
    {
        // Gera número aleatório
        var randomNumber = new byte[32];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(randomNumber);
        }
        // Retorna token
        return new RefreshToken
        {
            Token = Convert.ToBase64String(randomNumber),
            AddedDate = DateTime.UtcNow
        };
    }
}
```

---

## 6. ⚖️ Autorização com Roles e Claims

### Configuração de Policies

```csharp
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireAdminRole", policy => policy.RequireRole("Admin")); // Requer role "Admin"
    options.AddPolicy("CanViewProducts", policy => policy.RequireClaim("permission", "view:products")); // Requer claim
});
```

### Atribuição Dinâmica de Claims

```csharp
var user = await _userManager.FindByEmailAsync("user@example.com");
if (user != null)
{
    // Adiciona claim
    await _userManager.AddClaimAsync(user, new Claim("permission", "view:products"));
}
```

---

## 7. 🔒 Protegendo Endpoints

```csharp
[Authorize] // Requer autenticação
[HttpGet("protected-data")]
public IActionResult GetProtectedData()
{
    return Ok("Dados protegidos acessados!");
}

[Authorize(Roles = "Admin")] // Requer role "Admin"
[HttpGet("admin-only")]
public IActionResult GetAdminData()
{
    return Ok("Dados exclusivos para administradores.");
}

[Authorize(Policy = "CanViewProducts")] // Requer claim
[HttpGet("view-products")]
public IActionResult ViewProducts()
{
    return Ok("Lista de produtos.");
}

[AllowAnonymous] // Acesso público
[HttpGet("public-info")]
public IActionResult GetPublicInfo()
{
    return Ok("Informação pública.");
}
```

---

## 8. 📌 Boas Práticas e Segurança

- **Validação Rigorosa**: Use Data Annotations e valide `ModelState`.
- **Segurança de Chaves JWT**: Armazene chaves em variáveis de ambiente.
- **Expiração de Tokens**: JWTs curtos (30 minutos), refresh tokens longos (7 dias).
- **Revogação de Tokens**: Implemente revogação ao logout.
- **Proteção contra Ataques**: Mitigue força bruta, XSS, e injeção de SQL.
- **HTTPS**: Force HTTPS para dados sensíveis.
- **Logging**: Registre eventos de autenticação.
- **Testes**: Escreva testes unitários e de integração.
- **Documentação**: Use Swagger/OpenAPI.

---

## 9. 📋 Tabela de Endpoints

| Método | Endpoint                    | Descrição                              | Autenticação            |
|--------|-----------------------------|----------------------------------------|-------------------------|
| POST   | `/api/auth/register`        | Registra um novo usuário               | Anônimo                 |
| POST   | `/api/auth/login`           | Autentica o usuário e gera JWT         | Anônimo                 |
| POST   | `/api/auth/refresh-token`   | Renova o JWT usando refresh token      | Anônimo                 |
| POST   | `/api/auth/forgot-password` | Solicita recuperação de senha          | Anônimo                 |
| POST   | `/api/auth/reset-password`  | Redefine a senha do usuário            | Anônimo                 |
| POST   | `/api/auth/assign-role`     | Atribui uma role a um usuário          | Requer `RequireAdminRole` |
| POST   | `/api/auth/remove-role`     | Remove uma role de um usuário          | Requer `RequireAdminRole` |

