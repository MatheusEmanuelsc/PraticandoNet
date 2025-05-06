# 📄 Implementando JWT Authentication com ASP.NET Core Identity

## Sumário
1. [Introdução](#introdução)
2. [Configuração Inicial](#configuração-inicial)
   - [Instalação de Pacotes](#instalação-de-pacotes)
   - [Configuração de Secrets](#configuração-de-secrets)
3. [Implementação do JWT](#implementação-do-jwt)
   - [Criação do JwtConfig](#criação-do-jwtconfig)
   - [Configuração do Middleware](#configuração-do-middleware)
   - [Criação do TokenService](#criação-do-tokenservice)
4. [Controllers de Autenticação](#controllers-de-autenticação)
   - [DTOs de Autenticação](#dtos-de-autenticação)
   - [AuthController](#authcontroller)
5. [Utilização de Tokens](#utilização-de-tokens)
   - [Proteção de Endpoints](#proteção-de-endpoints)
   - [Refresh Tokens](#refresh-tokens)
6. [Segurança e Boas Práticas](#segurança-e-boas-práticas)
7. [Considerações Finais](#considerações-finais)

## Introdução

JWT (JSON Web Tokens) é um padrão aberto para autenticação e transmissão segura de informações entre partes como objetos JSON. Em combinação com o ASP.NET Core Identity, JWT oferece uma solução robusta para autenticação em APIs RESTful, especialmente em aplicações modernas com front-end separado (SPA, mobile apps, etc).

Este tutorial amplia o anterior sobre ASP.NET Core Identity, focando na implementação de autenticação baseada em tokens JWT.

## Configuração Inicial

### Instalação de Pacotes

Adicione os pacotes NuGet necessários:

```bash
# Pacote principal para JWT
dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer

# Opcional: Ferramentas para manipulação de JWT
dotnet add package System.IdentityModel.Tokens.Jwt
```

### Configuração de Secrets

Configure as chaves JWT no arquivo `appsettings.json`:

```json
{
  "JwtSettings": {
    "Secret": "YourSecretKeyHere_MakeSureItIsLongEnough_AtLeast32Characters",
    "Issuer": "your-api.com",
    "Audience": "your-client-app",
    "AccessTokenExpirationMinutes": 15,
    "RefreshTokenExpirationDays": 7
  }
}
```

Para ambientes de produção, use o gerenciador de secrets ou variáveis de ambiente:

```bash
dotnet user-secrets set "JwtSettings:Secret" "YourSecretKeyHere_MakeSureItIsLongEnough_AtLeast32Characters"
```

## Implementação do JWT

### Criação do JwtConfig

Crie uma classe para configurações do JWT:

```csharp
// Auth/JwtConfig.cs
namespace YourNamespace.Auth
{
    public class JwtConfig
    {
        public string Secret { get; set; }
        public string Issuer { get; set; }
        public string Audience { get; set; }
        public int AccessTokenExpirationMinutes { get; set; }
        public int RefreshTokenExpirationDays { get; set; }
    }
}
```

### Configuração do Middleware

Configure o middleware de autenticação JWT no `Program.cs`:

```csharp
// Program.cs (adicionando ao código existente)
// Carregar configurações JWT
builder.Services.Configure<JwtConfig>(builder.Configuration.GetSection("JwtSettings"));
var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtConfig>();
var key = Encoding.ASCII.GetBytes(jwtSettings.Secret);

// Configurar autenticação JWT
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false; // Definir como true em produção
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidIssuer = jwtSettings.Issuer,
        ValidAudience = jwtSettings.Audience,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});

// Adicionar autorização
builder.Services.AddAuthorization();

// Adicionar o serviço de token
builder.Services.AddScoped<ITokenService, TokenService>();
```

Certifique-se de adicionar os namespaces necessários:

```csharp
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using YourNamespace.Auth;
using YourNamespace.Services;
```

### Criação do TokenService

Implemente o serviço responsável por criar e validar tokens:

```csharp
// Services/TokenService.cs
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using YourNamespace.Auth;
using YourNamespace.Models.Identity;

namespace YourNamespace.Services
{
    public interface ITokenService
    {
        Task<string> GenerateAccessToken(ApplicationUser user);
        Task<string> GenerateRefreshToken();
        ClaimsPrincipal GetPrincipalFromExpiredToken(string token);
    }

    public class TokenService : ITokenService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly JwtConfig _jwtConfig;

        public TokenService(
            UserManager<ApplicationUser> userManager,
            IOptions<JwtConfig> jwtConfig)
        {
            _userManager = userManager;
            _jwtConfig = jwtConfig.Value;
        }

        public async Task<string> GenerateAccessToken(ApplicationUser user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_jwtConfig.Secret);
            
            var userRoles = await _userManager.GetRolesAsync(user);
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            // Adicionar roles como claims
            foreach (var role in userRoles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(_jwtConfig.AccessTokenExpirationMinutes),
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature),
                Issuer = _jwtConfig.Issuer,
                Audience = _jwtConfig.Audience
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        public async Task<string> GenerateRefreshToken()
        {
            return await Task.FromResult(Convert.ToBase64String(Guid.NewGuid().ToByteArray()));
        }

        public ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
        {
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_jwtConfig.Secret)),
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidIssuer = _jwtConfig.Issuer,
                ValidAudience = _jwtConfig.Audience,
                ValidateLifetime = false // Não validar expiração aqui
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out SecurityToken securityToken);
            
            if (securityToken is not JwtSecurityToken jwtSecurityToken || 
                !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            {
                throw new SecurityTokenException("Token inválido");
            }

            return principal;
        }
    }
}
```

## Controllers de Autenticação

### DTOs de Autenticação

Crie DTOs para as requisições de autenticação:

```csharp
// Models/Auth/AuthenticationDtos.cs
namespace YourNamespace.Models.Auth
{
    public class RegisterDto
    {
        public string UserName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string FullName { get; set; }
    }

    public class LoginDto
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }

    public class AuthResultDto
    {
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public bool Success { get; set; }
        public List<string> Errors { get; set; }
    }

    public class RefreshTokenDto
    {
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
    }
}
```

### AuthController

Crie um controller para gerenciar autenticação:

```csharp
// Controllers/AuthController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;
using YourNamespace.Models.Auth;
using YourNamespace.Models.Identity;
using YourNamespace.Services;

namespace YourNamespace.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ITokenService _tokenService;

        public AuthController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ITokenService tokenService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _tokenService = tokenService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new AuthResultDto
                {
                    Success = false,
                    Errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList()
                });
            }

            var user = new ApplicationUser
            {
                UserName = model.UserName,
                Email = model.Email,
                FullName = model.FullName
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (!result.Succeeded)
            {
                return BadRequest(new AuthResultDto
                {
                    Success = false,
                    Errors = result.Errors.Select(e => e.Description).ToList()
                });
            }

            // Adicionar o usuário a um role padrão (opcional)
            await _userManager.AddToRoleAsync(user, "User");

            // Gerar tokens
            var accessToken = await _tokenService.GenerateAccessToken(user);
            var refreshToken = await _tokenService.GenerateRefreshToken();

            // Armazenar refresh token no banco de dados (exemplo simplificado)
            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
            await _userManager.UpdateAsync(user);

            return Ok(new AuthResultDto
            {
                Success = true,
                AccessToken = accessToken,
                RefreshToken = refreshToken
            });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                return Unauthorized(new AuthResultDto
                {
                    Success = false,
                    Errors = new List<string> { "Credenciais inválidas" }
                });
            }

            var result = await _signInManager.CheckPasswordSignInAsync(user, model.Password, false);
            if (!result.Succeeded)
            {
                return Unauthorized(new AuthResultDto
                {
                    Success = false,
                    Errors = new List<string> { "Credenciais inválidas" }
                });
            }

            var accessToken = await _tokenService.GenerateAccessToken(user);
            var refreshToken = await _tokenService.GenerateRefreshToken();

            // Armazenar refresh token
            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
            await _userManager.UpdateAsync(user);

            return Ok(new AuthResultDto
            {
                Success = true,
                AccessToken = accessToken,
                RefreshToken = refreshToken
            });
        }

        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenDto model)
        {
            if (model is null)
            {
                return BadRequest("Requisição inválida");
            }

            var principal = _tokenService.GetPrincipalFromExpiredToken(model.AccessToken);
            var username = principal.Identity.Name;

            var user = await _userManager.FindByNameAsync(username);
            if (user == null || user.RefreshToken != model.RefreshToken || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
            {
                return BadRequest("Requisição de refresh token inválida");
            }

            var newAccessToken = await _tokenService.GenerateAccessToken(user);
            var newRefreshToken = await _tokenService.GenerateRefreshToken();

            user.RefreshToken = newRefreshToken;
            await _userManager.UpdateAsync(user);

            return Ok(new AuthResultDto
            {
                Success = true,
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken
            });
        }

        [Authorize]
        [HttpPost("revoke")]
        public async Task<IActionResult> Revoke()
        {
            var username = User.Identity.Name;
            var user = await _userManager.FindByNameAsync(username);
            if (user == null)
            {
                return BadRequest();
            }

            user.RefreshToken = null;
            await _userManager.UpdateAsync(user);

            return NoContent();
        }
    }
}
```

**Nota**: Adicione a propriedade `RefreshToken` e `RefreshTokenExpiryTime` à classe `ApplicationUser` criada anteriormente:

```csharp
// Atualização da classe ApplicationUser
public class ApplicationUser : IdentityUser
{
    public string FullName { get; set; }
    public string RefreshToken { get; set; }
    public DateTime RefreshTokenExpiryTime { get; set; }
}
```

## Utilização de Tokens

### Proteção de Endpoints

Proteja seus endpoints com o atributo `[Authorize]`:

```csharp
// Controllers/SomeController.cs
[ApiController]
[Route("api/[controller]")]
public class SomeController : ControllerBase
{
    [HttpGet]
    [Authorize] // Requer autenticação
    public IActionResult Get()
    {
        return Ok("Dados protegidos");
    }
    
    [HttpGet("admin")]
    [Authorize(Roles = "Admin")] // Requer role específica
    public IActionResult GetAdmin()
    {
        return Ok("Dados de administrador");
    }
    
    [HttpGet("policy")]
    [Authorize(Policy = "RequireAdminRole")] // Usando policies
    public IActionResult GetByPolicy()
    {
        return Ok("Dados protegidos por policy");
    }
}
```

### Refresh Tokens

O processo de refresh token já está implementado no `AuthController` acima, mas para utilizá-lo no frontend:

1. Armazene o access token e refresh token após login/registro
2. Utilize o access token para todas as requisições autenticadas
3. Quando receber erro 401 (Unauthorized), utilize o endpoint de refresh token
4. Atualize os tokens armazenados com os novos tokens
5. Refaça a requisição original com o novo access token

## Segurança e Boas Práticas

1. **Proteção de chaves**: Nunca armazene a chave secreta JWT diretamente no código ou em repositórios.

2. **Configuração de Expiração**:
   - Access tokens devem ter vida curta (15-30 minutos)
   - Refresh tokens podem ter vida mais longa (dias ou semanas)

3. **Validação Completa**:
   - Sempre valide Issuer, Audience e Lifetime em produção
   - Use HTTPS para transmissão de tokens

4. **Implementação de Logout**:
   - Invalidar refresh tokens no servidor
   - Manter uma lista de tokens invalidados (blacklist) ou usar um mecanismo de versão

5. **Armazenamento de Tokens no Cliente**:
   - Access token: Memória da aplicação (não localStorage em aplicações web)
   - Refresh token: HttpOnly cookie ou armazenamento mais seguro

6. **Rotação de Chaves**:
   - Considere a rotação periódica das chaves de assinatura
   - Implemente um mecanismo para atualizar chaves sem interromper o serviço

## Considerações Finais

A implementação de JWT com ASP.NET Core Identity fornece uma solução robusta e flexível para autenticação e autorização em APIs modernas. Entretanto, é fundamental manter-se atualizado com as melhores práticas de segurança e considerar as necessidades específicas do seu sistema.

Alguns pontos adicionais a considerar:

- Implementação de duas etapas de autenticação (2FA) para maior segurança
- Monitoramento e logging de atividades de autenticação
- Implementação de limites de tentativas de login para prevenir ataques de força bruta
- Rotação periódica de refresh tokens
- Uso de claims personalizadas para autorização mais granular

Seguindo estas diretrizes, você terá uma implementação de JWT segura e escalável para sua aplicação ASP.NET Core.