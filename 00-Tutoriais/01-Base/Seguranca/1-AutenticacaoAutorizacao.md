

# 🔐 Autenticação e Autorização com ASP.NET Core 8 (Identity + JWT)

Este tutorial completo e atualizado explica como implementar um sistema robusto de autenticação e autorização utilizando **ASP.NET Core 8**, **Identity**, **JWT**, **Refresh Tokens**, **Confirmação de E-mail**, **Recuperação de Senha** e **Autorização com Roles e Claims**. O código segue boas práticas de segurança e modularidade, com exemplos práticos para cada etapa.

---

## 📘 Índice

1. [Pacotes Necessários](#1-pacotes-necessários)  
2. [Configuração do Identity](#2-configuração-do-identity)  
3. [Configuração do JWT](#3-configuração-do-jwt)  
4. [Models: ApplicationUser e RefreshToken](#4-models-applicationuser-e-refreshtoken)  
5. [DTOs para Autenticação](#5-dtos-para-autenticação)  
6. [Controller de Usuários](#6-controller-de-usuários)  
7. [Autorização com Roles e Claims](#7-autorização-com-roles-e-claims)  
8. [Protegendo Rotas](#8-protegendo-rotas)  
9. [Boas Práticas e Considerações Finais](#9-boas-práticas-e-considerações-finais)

---

## 1. 📦 Pacotes Necessários

Adicione os seguintes pacotes ao seu projeto:

```bash
dotnet add package Microsoft.AspNetCore.Identity.EntityFrameworkCore
dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer
dotnet add package Microsoft.EntityFrameworkCore.SqlServer
```

---

## 2. ⚙️ Configuração do Identity

No arquivo `Program.cs`, configure o **Identity** para gerenciar usuários e roles:

```csharp
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.SignIn.RequireConfirmedEmail = true;
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 8;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();
```

---

## 3. 🔑 Configuração do JWT

### `appsettings.json`:

```json
{
  "Jwt": {
    "Key": "sua-chave-secreta-de-32-caracteres-ou-mais",
    "Issuer": "MinhaApi",
    "Audience": "ClientesDaMinhaApi"
  }
}
```

### `Program.cs`:

```csharp
var configuration = builder.Configuration;

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidIssuer = configuration["Jwt:Issuer"],
        ValidateAudience = true,
        ValidAudience = configuration["Jwt:Audience"],
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Key"])),
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization();
```

---

## 4. 👤 Models: `ApplicationUser` e `RefreshToken`

### `ApplicationUser`:

```csharp
public class ApplicationUser : IdentityUser
{
    public string NomeCompleto { get; set; } = string.Empty;
}
```

### `RefreshToken`:

```csharp
public class RefreshToken
{
    public int Id { get; set; }
    public string Token { get; set; } = null!;
    public string UserId { get; set; } = null!;
    public ApplicationUser User { get; set; } = null!;
    public DateTime Expiration { get; set; }
    public bool IsRevoked { get; set; }
}
```

---

## 5. 📝 DTOs para Autenticação

Os DTOs garantem que apenas os dados necessários sejam expostos.

### `RegisterDTO`:

```csharp
public class RegisterDTO
{
    [Required]
    public string UserName { get; set; } = null!;
    
    [Required]
    [EmailAddress]
    public string Email { get; set; } = null!;
    
    [Required]
    [MinLength(8)]
    public string Password { get; set; } = null!;
    
    [Required]
    public string NomeCompleto { get; set; } = null!;
}
```

### `LoginDTO`:

```csharp
public class LoginDTO
{
    [Required]
    public string UserName { get; set; } = null!;
    
    [Required]
    public string Password { get; set; } = null!;
}
```

### `RefreshTokenDTO`:

```csharp
public class RefreshTokenDTO
{
    [Required]
    public string RefreshToken { get; set; } = null!;
}
```

### `ResetPasswordDTO`:

```csharp
public class ResetPasswordDTO
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = null!;
    
    [Required]
    public string Token { get; set; } = null!;
    
    [Required]
    [MinLength(8)]
    public string NovaSenha { get; set; } = null!;
}
```

### `RespuestaAutenticacionDTO`:

```csharp
public class RespuestaAutenticacionDTO
{
    public string Token { get; set; } = null!;
    public string RefreshToken { get; set; } = null!;
    public DateTime Expiracion { get; set; }
}
```

---

## 6. 🎮 Controller de Usuários

O controller foi adaptado para seguir as boas práticas e incorporar todas as funcionalidades descritas, incluindo **registro**, **login**, **refresh token**, **recuperação de senha** e **gerenciamento de roles/claims**.

```csharp
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace SuaApi.Controllers
{
    [ApiController]
    [Route("api/usuarios")]
    public class UsuariosController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IConfiguration _configuration;
        private readonly RoleManager<IdentityRole> _roleManager;

        public UsuariosController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IConfiguration configuration,
            RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
            _roleManager = roleManager;
        }

        [HttpPost("registro")]
        [AllowAnonymous]
        public async Task<IActionResult> Registrar(RegisterDTO dto)
        {
            var user = new ApplicationUser
            {
                UserName = dto.UserName,
                Email = dto.Email,
                NomeCompleto = dto.NomeCompleto
            };

            var result = await _userManager.CreateAsync(user, dto.Password);
            if (!result.Succeeded)
                return BadRequest(result.Errors);

            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            // TODO: Enviar e-mail com o token para confirmação

            return Ok(new { Message = "Registro bem-sucedido. Verifique seu e-mail." });
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login(LoginDTO dto)
        {
            var user = await _userManager.FindByNameAsync(dto.UserName);
            if (user == null || !await _userManager.CheckPasswordAsync(user, dto.Password))
                return Unauthorized(new { Message = "Credenciais inválidas" });

            var tokenResult = await GenerateJwtToken(user);
            return Ok(tokenResult);
        }

        [HttpPost("refresh-token")]
        [AllowAnonymous]
        public async Task<IActionResult> RefreshToken(RefreshTokenDTO dto)
        {
            // Simulação de busca no banco de dados (substituir por repositório real)
            var storedToken = await Task.FromResult(new RefreshToken { Token = dto.RefreshToken, UserId = "user-id", Expiration = DateTime.UtcNow.AddDays(1), IsRevoked = false });
            if (storedToken == null || storedToken.IsRevoked || storedToken.Expiration < DateTime.Now)
                return Unauthorized(new { Message = "Refresh token inválido ou expirado" });

            var user = await _userManager.FindByIdAsync(storedToken.UserId);
            if (user == null)
                return Unauthorized(new { Message = "Usuário não encontrado" });

            var tokenResult = await GenerateJwtToken(user);
            return Ok(tokenResult);
        }

        [HttpPost("esqueci-senha")]
        [AllowAnonymous]
        public async Task<IActionResult> EsqueciSenha([FromBody] string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                return BadRequest(new { Message = "Usuário não encontrado" });

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            // TODO: Enviar e-mail com o token de reset

            return Ok(new { Message = "E-mail de recuperação enviado" });
        }

        [HttpPost("resetar-senha")]
        [AllowAnonymous]
        public async Task<IActionResult> ResetarSenha(ResetPasswordDTO dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null)
                return BadRequest(new { Message = "Usuário não encontrado" });

            var result = await _userManager.ResetPasswordAsync(user, dto.Token, dto.NovaSenha);
            if (!result.Succeeded)
                return BadRequest(result.Errors);

            return Ok(new { Message = "Senha redefinida com sucesso" });
        }

        [HttpPost("hacer-admin")]
        [Authorize(Policy = "esadmin")]
        public async Task<IActionResult> HacerAdmin([FromBody] string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                return NotFound(new { Message = "Usuário não encontrado" });

            if (!await _roleManager.RoleExistsAsync("Admin"))
                await _roleManager.CreateAsync(new IdentityRole("Admin"));

            await _userManager.AddToRoleAsync(user, "Admin");
            await _userManager.AddClaimAsync(user, new Claim("esadmin", "true"));

            return NoContent();
        }

        [HttpPost("remover-admin")]
        [Authorize(Policy = "esadmin")]
        public async Task<IActionResult> RemoverAdmin([FromBody] string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                return NotFound(new { Message = "Usuário não encontrado" });

            await _userManager.RemoveFromRoleAsync(user, "Admin");
            await _userManager.RemoveClaimAsync(user, new Claim("esadmin", "true"));

            return NoContent();
        }

        private async Task<RespuestaAutenticacionDTO> GenerateJwtToken(ApplicationUser user)
        {
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.UserName),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim("NomeCompleto", user.NomeCompleto)
            };

            var userClaims = await _userManager.GetClaimsAsync(user);
            claims.AddRange(userClaims);

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(30),
                signingCredentials: creds);

            var refreshToken = Guid.NewGuid().ToString();
            // TODO: Salvar refreshToken no banco de dados

            return new RespuestaAutenticacionDTO
            {
                Token = new JwtSecurityTokenHandler().WriteToken(token),
                RefreshToken = refreshToken,
                Expiracion = token.ValidTo
            };
        }
    }
}
```

---

## 7. ⚖️ Autorização com Roles e Claims

### Configuração de Policies:

Adicione políticas de autorização no `Program.cs`:

```csharp
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("esadmin", policy => policy.RequireClaim("esadmin", "true"));
});
```

### Exemplo de Atribuição de Roles e Claims:

```csharp
var user = await _userManager.FindByNameAsync("admin");
await _userManager.AddToRoleAsync(user, "Admin");
await _userManager.AddClaimAsync(user, new Claim("esadmin", "true"));
```

---

## 8. 🔒 Protegendo Rotas

Use os atributos `[Authorize]` e `[AllowAnonymous]` para controlar o acesso:

```csharp
[Authorize]
[HttpGet("dados-protegidos")]
public IActionResult DadosProtegidos()
{
    return Ok("Rota protegida!");
}

[AllowAnonymous]
[HttpGet("publico")]
public IActionResult Publico()
{
    return Ok("Rota pública!");
}
```

---

## 9. 📌 Boas Práticas e Considerações Finais

- **Validação de Entrada**: Sempre valide DTOs usando atributos como `[Required]` e `[EmailAddress]`.
- **Segurança do JWT**: Use chaves fortes e configure tempos de expiração curtos para tokens.
- **Refresh Tokens**: Armazene refresh tokens de forma segura no banco de dados e implemente revogação.
- **E-mail**: Integre um serviço de e-mail (como SendGrid ou SMTP) para confirmação e recuperação de senha.
- **Erros**: Retorne mensagens de erro claras, mas evite expor detalhes sensíveis.
- **Testes**: Crie testes unitários para os endpoints de autenticação.


