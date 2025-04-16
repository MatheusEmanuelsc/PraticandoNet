
# 🔐 Autenticação e Autorização com ASP.NET Core 8 (Identity + JWT)

Este guia completo implementa um sistema robusto de autenticação e autorização em **ASP.NET Core 8** usando **Identity**, **JWT**, **Refresh Tokens**, **Confirmação de E-mail**, **Recuperação de Senha** e **Autorização com Roles e Claims**. O código é comentado detalhadamente para explicar o propósito de cada função e configuração, seguindo boas práticas de segurança e modularidade.

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

Adicione os pacotes necessários via NuGet para suportar Identity, autenticação JWT e Entity Framework:

```bash
dotnet add package Microsoft.AspNetCore.Identity.EntityFrameworkCore
dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer
dotnet add package Microsoft.EntityFrameworkCore.SqlServer
```

---

## 2. ⚙️ Configuração do Identity

No `Program.cs`, configuramos o **Identity** para gerenciar usuários e roles, com opções de segurança e tokens.

```csharp
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.SignIn.RequireConfirmedEmail = true; // Exige que o e-mail seja confirmado antes do login
    options.Password.RequireDigit = true; // Senha deve conter pelo menos um dígito
    options.Password.RequiredLength = 8; // Senha deve ter no mínimo 8 caracteres
})
.AddEntityFrameworkStores<ApplicationDbContext>() // Configura o Identity para usar o Entity Framework com o ApplicationDbContext
.AddDefaultTokenProviders(); // Habilita provedores padrão para geração de tokens (ex.: para confirmação de e-mail e redefinição de senha)
```

**Explicação de `AddDefaultTokenProviders`**:  
Este método registra provedores padrão do ASP.NET Identity para gerar tokens seguros usados em fluxos como confirmação de e-mail, redefinição de senha e autenticação de dois fatores. Ele permite que métodos como `GenerateEmailConfirmationTokenAsync` e `GeneratePasswordResetTokenAsync` funcionem, criando tokens temporários e criptograficamente seguros.

---

## 3. 🔑 Configuração do JWT

### `appsettings.json`:

Defina as configurações do JWT, incluindo chave secreta, emissor e audiência.

```json
{
  "Jwt": {
    "Key": "sua-chave-secreta-de-32-caracteres-ou-mais", // Chave secreta para assinar tokens JWT
    "Issuer": "MinhaApi", // Identifica o emissor do token
    "Audience": "ClientesDaMinhaApi" // Identifica os destinatários autorizados do token
  }
}
```

### `Program.cs`:

Configure a autenticação JWT com validação rigorosa.

```csharp
var configuration = builder.Configuration;

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme; // Define JWT como esquema padrão de autenticação
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme; // Define JWT como esquema para desafios de autenticação
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true, // Valida o emissor do token
        ValidIssuer = configuration["Jwt:Issuer"], // Emissor esperado
        ValidateAudience = true, // Valida a audiência do token
        ValidAudience = configuration["Jwt:Audience"], // Audiência esperada
        ValidateIssuerSigningKey = true, // Valida a chave de assinatura
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Key"])), // Chave secreta para verificar a assinatura
        ValidateLifetime = true, // Verifica se o token não está expirado
        ClockSkew = TimeSpan.Zero // Remove tolerância de expiração para maior precisão
    };
});

builder.Services.AddAuthorization(); // Habilita serviços de autorização para políticas e roles
```

**Explicação**:  
A configuração acima define o JWT como o mecanismo de autenticação, validando emissor, audiência, assinatura e expiração do token. `ClockSkew = TimeSpan.Zero` garante que tokens expirados sejam rejeitados imediatamente, sem margem de tolerância.

---

## 4. 👤 Models e DTOs

### Models

#### `ApplicationUser`:

```csharp
public class ApplicationUser : IdentityUser
{
    public string NomeCompleto { get; set; } = string.Empty; // Campo personalizado para armazenar o nome completo do usuário
}
```

**Explicação**:  
Herda de `IdentityUser` para incluir propriedades padrão como `Id`, `UserName` e `Email`, adicionando `NomeCompleto` como campo extra.

#### `RefreshToken`:

```csharp
public class RefreshToken
{
    public int Id { get; set; } // Identificador único no banco
    public string Token { get; set; } = null!; // Valor do refresh token
    public string UserId { get; set; } = null!; // ID do usuário associado
    public string JwtId { get; set; } = null!; // ID do JWT associado para rastreamento
    public DateTime AddedDate { get; set; } // Data de criação
    public DateTime Expiration { get; set; } // Data de expiração
    public bool IsUsed { get; set; } // Indica se o token já foi usado
    public bool IsRevoked { get; set; } // Indica se o token foi revogado
}
```

**Explicação**:  
Armazena refresh tokens no banco de dados, permitindo validação e revogação. `JwtId` vincula o refresh token a um JWT específico.

### DTOs

#### `RegisterDTO`:

```csharp
public class RegisterDTO
{
    [Required]
    public string UserName { get; set; } = null!; // Nome de usuário obrigatório
    [Required, EmailAddress]
    public string Email { get; set; } = null!; // E-mail válido e obrigatório
    [Required, MinLength(8)]
    public string Password { get; set; } = null!; // Senha com mínimo de 8 caracteres
    [Required]
    public string NomeCompleto { get; set; } = null!; // Nome completo obrigatório
}
```

**Explicação**:  
Usado para receber dados de registro, com validações para garantir entradas corretas.

#### `LoginDTO`:

```csharp
public class LoginDTO
{
    [Required]
    public string UserName { get; set; } = null!; // Nome de usuário para login
    [Required]
    public string Password { get; set; } = null!; // Senha para autenticação
}
```

**Explicação**:  
Recebe credenciais de login, com validações para campos obrigatórios.

#### `RefreshTokenDTO`:

```csharp
public class RefreshTokenDTO
{
    [Required]
    public string RefreshToken { get; set; } = null!; // Refresh token para renovação do JWT
}
```

**Explicação**:  
Usado para enviar o refresh token ao solicitar um novo JWT.

#### `ResetPasswordDTO`:

```csharp
public class ResetPasswordDTO
{
    [Required, EmailAddress]
    public string Email { get; set; } = null!; // E-mail do usuário
    [Required]
    public string Token { get; set; } = null!; // Token de redefinição de senha
    [Required, MinLength(8)]
    public string NovaSenha { get; set; } = null!; // Nova senha com mínimo de 8 caracteres
}
```

**Explicação**:  
Recebe dados para redefinir a senha, com validações para e-mail e senha.

#### `RespuestaAutenticacionDTO`:

```csharp
public class RespuestaAutenticacionDTO
{
    public string Token { get; set; } = null!; // JWT gerado
    public string RefreshToken { get; set; } = null!; // Refresh token associado
    public DateTime Expiracion { get; set; } // Data de expiração do JWT
}
```

**Explicação**:  
Retorna o JWT, o refresh token e a data de expiração após login ou renovação.

---

## 5. 🎮 AuthController

O `AuthController` implementa endpoints para autenticação e gerenciamento de usuários, com comentários explicando cada método.

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

// Controlador para autenticação e gerenciamento de usuários
[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager; // Gerencia usuários (criação, busca, etc.)
    private readonly RoleManager<IdentityRole> _roleManager; // Gerencia roles
    private readonly IConfiguration _configuration; // Acessa configurações (ex.: Jwt:Key)
    private readonly ApplicationDbContext _context; // Contexto do banco para armazenar refresh tokens

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

    // Registra um novo usuário e envia um link de confirmação de e-mail
    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register(RegisterDTO dto)
    {
        // Cria um novo usuário com os dados fornecidos
        var user = new ApplicationUser
        {
            UserName = dto.UserName,
            Email = dto.Email,
            NomeCompleto = dto.NomeCompleto
        };

        // Salva o usuário no banco com a senha fornecida
        var result = await _userManager.CreateAsync(user, dto.Password);
        if (!result.Succeeded)
            return BadRequest(new { Errors = result.Errors }); // Retorna erros se a criação falhar

        // Gera um token para confirmação de e-mail
        var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
        // Cria um link para o endpoint de confirmação
        var confirmLink = Url.Action("ConfirmEmail", "Auth", new { userId = user.Id, token }, Request.Scheme);
        // TODO: Enviar e-mail com confirmLink (ex.: via SendGrid)
        Console.WriteLine($"Link de confirmação: {confirmLink}"); // Para testes locais

        return Ok(new { Message = "Registro bem-sucedido. Verifique seu e-mail." });
    }

    // Confirma o e-mail do usuário usando o token recebido
    [HttpGet("confirm-email")]
    [AllowAnonymous]
    public async Task<IActionResult> ConfirmEmail(string userId, string token)
    {
        // Valida parâmetros do link
        if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(token))
            return BadRequest(new { Message = "Link de confirmação inválido." });

        // Busca o usuário pelo ID
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return NotFound(new { Message = "Usuário não encontrado." });

        // Confirma o e-mail usando o token
        var result = await _userManager.ConfirmEmailAsync(user, token);
        if (result.Succeeded)
            return Ok(new { Message = "E-mail confirmado com sucesso!" });
        return BadRequest(new { Errors = result.Errors }); // Retorna erros se a confirmação falhar
    }

    // Autentica o usuário e retorna um JWT e refresh token
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login(LoginDTO dto)
    {
        // Busca o usuário pelo nome de usuário
        var user = await _userManager.FindByNameAsync(dto.UserName);
        // Verifica se o usuário existe, a senha está correta e o e-mail foi confirmado
        if (user == null || !await _userManager.CheckPasswordAsync(user, dto.Password) || !await _userManager.IsEmailConfirmedAsync(user))
            return Unauthorized(new { Message = "Credenciais inválidas ou e-mail não confirmado." });

        // Gera JWT e refresh token
        var tokenResult = await GenerateJwtToken(user);
        return Ok(tokenResult); // Retorna o token e refresh token
    }

    // Renova o JWT usando um refresh token válido
    [HttpPost("refresh-token")]
    [AllowAnonymous]
    public async Task<IActionResult> RefreshToken(RefreshTokenDTO dto)
    {
        // Busca o refresh token no banco
        var refreshToken = await _context.RefreshTokens.FirstOrDefaultAsync(rt => rt.Token == dto.RefreshToken);
        // Verifica se o token existe, não foi revogado, não foi usado e não está expirado
        if (refreshToken == null || refreshToken.IsRevoked || refreshToken.IsUsed || refreshToken.Expiration < DateTime.UtcNow)
            return BadRequest(new { Message = "Refresh token inválido." });

        // Busca o usuário associado ao refresh token
        var user = await _userManager.FindByIdAsync(refreshToken.UserId);
        if (user == null)
            return NotFound(new { Message = "Usuário não encontrado." });

        // Marca o refresh token como usado
        refreshToken.IsUsed = true;
        _context.Update(refreshToken);
        await _context.SaveChangesAsync();

        // Gera um novo JWT e refresh token
        var jwtResult = await GenerateJwtToken(user);
        return Ok(jwtResult);
    }

    // Inicia o processo de recuperação de senha, enviando um link de redefinição
    [HttpPost("forgot-password")]
    [AllowAnonymous]
    public async Task<IActionResult> ForgotPassword([FromBody] string email)
    {
        // Busca o usuário pelo e-mail
        var user = await _userManager.FindByEmailAsync(email);
        // Retorna uma mensagem genérica para evitar expor se o e-mail existe
        if (user == null || !await _userManager.IsEmailConfirmedAsync(user))
            return Ok(new { Message = "Se o e-mail estiver cadastrado, um link de recuperação será enviado." });

        // Gera um token para redefinição de senha
        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        // Cria um link para o endpoint de redefinição
        var resetLink = Url.Action("ResetPassword", "Auth", new { userId = user.Id, token }, Request.Scheme);
        // TODO: Enviar e-mail com resetLink (ex.: via SendGrid)
        Console.WriteLine($"Link de redefinição: {resetLink}"); // Para testes locais

        return Ok(new { Message = "Link de recuperação enviado." });
    }

    // Redefine a senha do usuário usando o token de redefinição
    [HttpPost("reset-password")]
    [AllowAnonymous]
    public async Task<IActionResult> ResetPassword(ResetPasswordDTO dto)
    {
        // Busca o usuário pelo e-mail
        var user = await _userManager.FindByEmailAsync(dto.Email);
        if (user == null)
            return BadRequest(new { Message = "Usuário não encontrado." });

        // Redefine a senha usando o token fornecido
        var result = await _userManager.ResetPasswordAsync(user, dto.Token, dto.NovaSenha);
        if (result.Succeeded)
            return Ok(new { Message = "Senha redefinida com sucesso!" });
        return BadRequest(new { Errors = result.Errors }); // Retorna erros se a redefinição falhar
    }

    // Atribui uma role a um usuário (restrito a administradores)
    [HttpPost("assign-role")]
    [Authorize(Policy = "RequireAdminRole")]
    public async Task<IActionResult> AssignRole(string email, string roleName)
    {
        // Busca o usuário pelo e-mail
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
            return NotFound(new { Message = "Usuário não encontrado." });

        // Cria a role se ela não existir
        if (!await _roleManager.RoleExistsAsync(roleName))
            await _roleManager.CreateAsync(new IdentityRole(roleName));

        // Adiciona o usuário à role
        var result = await _userManager.AddToRoleAsync(user, roleName);
        if (result.Succeeded)
            return Ok(new { Message = $"Usuário {email} adicionado à role {roleName}." });
        return BadRequest(new { Errors = result.Errors }); // Retorna erros se a atribuição falhar
    }

    // Remove uma role de um usuário (restrito a administradores)
    [HttpPost("remove-role")]
    [Authorize(Policy = "RequireAdminRole")]
    public async Task<IActionResult> RemoveRole(string email, string roleName)
    {
        // Busca o usuário pelo e-mail
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
            return NotFound(new { Message = "Usuário não encontrado." });

        // Remove o usuário da role
        var result = await _userManager.RemoveFromRoleAsync(user, roleName);
        if (result.Succeeded)
            return Ok(new { Message = $"Usuário {email} removido da role {roleName}." });
        return BadRequest(new { Errors = result.Errors }); // Retorna erros se a remoção falhar
    }

    // Gera um JWT e um refresh token para o usuário
    private async Task<RespuestaAutenticacionDTO> GenerateJwtToken(ApplicationUser user)
    {
        // Define as claims do JWT (informações do usuário)
        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.UserName), // Identificador do usuário
            new Claim(JwtRegisteredClaimNames.Email, user.Email), // E-mail do usuário
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()), // ID único do token
            new Claim("NomeCompleto", user.NomeCompleto), // Nome completo
            new Claim("Id", user.Id) // ID do usuário
        };

        // Adiciona roles do usuário como claims
        var userRoles = await _userManager.GetRolesAsync(user);
        claims.AddRange(userRoles.Select(role => new Claim(ClaimTypes.Role, role)));

        // Adiciona claims personalizadas do usuário
        var userClaims = await _userManager.GetClaimsAsync(user);
        claims.AddRange(userClaims);

        // Configura a chave de assinatura do JWT
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expiry = DateTime.UtcNow.AddMinutes(30); // Expiração do JWT (30 minutos)

        // Cria o JWT
        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: expiry,
            signingCredentials: creds);

        // Gera e salva um novo refresh token
        var refreshToken = GenerateRefreshToken();
        refreshToken.UserId = user.Id;
        refreshToken.JwtId = token.Id;
        refreshToken.Expiration = DateTime.UtcNow.AddDays(7); // Expiração do refresh token (7 dias)
        await _context.RefreshTokens.AddAsync(refreshToken);
        await _context.SaveChangesAsync();

        // Retorna o JWT, refresh token e data de expiração
        return new RespuestaAutenticacionDTO
        {
            Token = new JwtSecurityTokenHandler().WriteToken(token),
            RefreshToken = refreshToken.Token,
            Expiracion = expiry
        };
    }

    // Gera um refresh token seguro
    private RefreshToken GenerateRefreshToken()
    {
        // Gera um número aleatório de 32 bytes para o token
        var randomNumber = new byte[32];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(randomNumber); // Preenche o array com bytes aleatórios
        }
        // Retorna o refresh token com o valor codificado
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

Adicione políticas de autorização no `Program.cs` para restringir acesso baseado em roles e claims.

```csharp
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireAdminRole", policy => policy.RequireRole("Admin")); // Requer a role "Admin"
    options.AddPolicy("CanViewProducts", policy => policy.RequireClaim("permission", "view:products")); // Requer a claim "view:products"
});
```

**Explicação**:  
Políticas permitem regras complexas de autorização. `RequireAdminRole` exige que o usuário tenha a role "Admin", enquanto `CanViewProducts` exige uma claim específica.

### Atribuição Dinâmica de Claims

Exemplo de adição de uma claim a um usuário:

```csharp
var user = await _userManager.FindByEmailAsync("user@example.com");
if (user != null)
{
    // Adiciona uma claim personalizada ao usuário
    await _userManager.AddClaimAsync(user, new Claim("permission", "view:products"));
}
```

**Explicação**:  
Claims são atribuídas dinamicamente para conceder permissões específicas, armazenadas no banco e incluídas no JWT.

---

## 7. 🔒 Protegendo Endpoints

Use atributos para controlar o acesso aos endpoints:

```csharp
[Authorize] // Requer autenticação
[HttpGet("protected-data")]
public IActionResult GetProtectedData()
{
    return Ok("Dados protegidos acessados!");
}

[Authorize(Roles = "Admin")] // Requer a role "Admin"
[HttpGet("admin-only")]
public IActionResult GetAdminData()
{
    return Ok("Dados exclusivos para administradores.");
}

[Authorize(Policy = "CanViewProducts")] // Requer a claim "view:products"
[HttpGet("view-products")]
public IActionResult ViewProducts()
{
    return Ok("Lista de produtos.");
}

[AllowAnonymous] // Permite acesso sem autenticação
[HttpGet("public-info")]
public IActionResult GetPublicInfo()
{
    return Ok("Informação pública.");
}
```

**Explicação**:  
`[Authorize]` protege endpoints, exigindo um JWT válido. `Roles` e `Policy` adicionam restrições baseadas em roles ou claims, enquanto `[AllowAnonymous]` permite acesso público.

---

## 8. 📌 Boas Práticas e Segurança

- **Validação Rigorosa**: Use Data Annotations (`[Required]`, `[EmailAddress]`) e valide `ModelState` nos controllers para entradas seguras.
- **Segurança de Chaves JWT**: Armazene a chave JWT em variáveis de ambiente ou serviços de gerenciamento de segredos.
- **Expiração de Tokens**: Use tempos curtos para JWTs (ex.: 30 minutos) e mais longos para refresh tokens (ex.: 7 dias), com revogação.
- **Revogação de Tokens**: Implemente revogação de refresh tokens ao logout ou em caso de comprometimento.
- **Proteção contra Ataques**: Mitigue força bruta, XSS e injeção de SQL com validações e proteções do ASP.NET Core.
- **HTTPS**: Force HTTPS para proteger dados em trânsito.
- **E-mail**: Integre serviços como SendGrid para envio de e-mails. Exemplo:

```csharp
var client = new SendGridClient("sua-chave-api"); // Cliente SendGrid com chave API
var msg = new SendGridMessage
{
    From = new EmailAddress("no-reply@suaapi.com", "Sua API"), // Remetente
    Subject = "Confirme seu e-mail", // Assunto do e-mail
    PlainTextContent = $"Clique aqui: {confirmLink}" // Corpo do e-mail com link
};
msg.AddTo(new EmailAddress(user.Email)); // Destinatário
await client.SendEmailAsync(msg); // Envia o e-mail
```

- **Logging e Auditoria**: Registre eventos de autenticação para monitoramento.
- **Testes**: Escreva testes unitários e de integração para endpoints.
- **Documentação**: Use Swagger/OpenAPI para documentar a API.

---

## 9. 📋 Tabela de Endpoints

| Método | Endpoint                        | Descrição                              | Autenticação            |
|--------|---------------------------------|----------------------------------------|-------------------------|
| POST   | `/api/auth/register`            | Registra um novo usuário               | Anônimo                 |
| GET    | `/api/auth/confirm-email`       | Confirma o e-mail do usuário           | Anônimo                 |
| POST   | `/api/auth/login`               | Autentica o usuário e gera JWT         | Anônimo                 |
| POST   | `/api/auth/refresh-token`       | Renova o JWT usando refresh token      | Anônimo                 |
| POST   | `/api/auth/forgot-password`     | Solicita recuperação de senha          | Anônimo                 |
| POST   | `/api/auth/reset-password`      | Redefine a senha do usuário            | Anônimo                 |
| POST   | `/api/auth/assign-role`         | Atribui uma role a um usuário          | Requer `RequireAdminRole` |
| POST   | `/api/auth/remove-role`         | Remove uma role de um usuário          | Requer `RequireAdminRole` |

