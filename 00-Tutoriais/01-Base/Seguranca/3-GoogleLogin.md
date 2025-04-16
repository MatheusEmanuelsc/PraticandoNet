

# 🔑 Login com Google no ASP.NET Core 8 (OAuth + JWT)

Este guia detalha como implementar o **login com Google** em uma **Web API ASP.NET Core 8** usando **OAuth 2.0** e **Identity**, gerando um **JWT** após autenticação. O código é comentado para clareza, integrado com o Identity para gerenciar usuários, e formatado para renderização correta no GitHub.

## 📘 Índice

1. Pacotes Necessários
2. Configuração no Google Cloud Console
3. Configuração do ASP.NET Core
4. Modelos e DTOs
5. AuthController (Login com Google)
6. Boas Práticas e Segurança
7. Tabela de Endpoints

---

## 1. 📦 Pacotes Necessários

Adicione os pacotes via NuGet para suportar Identity, autenticação JWT e OAuth do Google:

```bash
dotnet add package Microsoft.AspNetCore.Identity.EntityFrameworkCore
dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer
dotnet add package Microsoft.AspNetCore.Authentication.Google
dotnet add package Microsoft.EntityFrameworkCore.SqlServer
```

**Explicação**:  
- `Microsoft.AspNetCore.Identity.EntityFrameworkCore`: Gerencia usuários e autenticação.  
- `Microsoft.AspNetCore.Authentication.JwtBearer`: Valida JWTs gerados após login.  
- `Microsoft.AspNetCore.Authentication.Google`: Integra autenticação OAuth do Google.  
- `Microsoft.EntityFrameworkCore.SqlServer`: Persiste dados do Identity.

---

## 2. 🔧 Configuração no Google Cloud Console

Crie credenciais OAuth para sua aplicação:

1. Acesse o [Google Cloud Console](https://console.cloud.google.com/).
2. Crie um projeto (ex.: "MinhaApiGoogleLogin").
3. Vá para **APIs & Services** > **Credentials**.
4. Clique em **Create Credentials** > **OAuth 2.0 Client IDs**.
5. Configure:
   - **Application type**: Web application.
   - **Name**: Ex.: "Minha API".
   - **Authorized JavaScript origins**: Adicione `https://localhost:5001` (ou sua URL).
   - **Authorized redirect URIs**: Adicione `https://localhost:5001/signin-google` (endpoint de callback).
6. Salve e copie o **Client ID** e **Client Secret**.

**Explicação**:  
O Google usa essas credenciais para autenticar sua aplicação e redirecionar usuários após o login.

---

## 3. ⚙️ Configuração do ASP.NET Core

### `appsettings.json`

Adicione as configurações do Google e JWT:

```json
{
  "Authentication": {
    "Google": {
      "ClientId": "SUA-GOOGLE-CLIENT-ID", // Client ID do Google
      "ClientSecret": "SUA-GOOGLE-CLIENT-SECRET" // Client Secret do Google
    }
  },
  "Jwt": {
    "Key": "sua-chave-secreta-de-32-caracteres-ou-mais", // Chave para JWT
    "Issuer": "MinhaApi", // Emissor do JWT
    "Audience": "ClientesDaMinhaApi" // Audiência do JWT
  }
}
```

**Explicação**:  
- `Google:ClientId` e `ClientSecret`: Credenciais do Google OAuth.  
- `Jwt`: Configurações para gerar tokens JWT após login.

### `Program.cs`

Configure o Identity, autenticação Google, e JWT:

```csharp
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Configura o DbContext para Identity
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Configura o Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = true; // Senha deve conter dígito
    options.Password.RequiredLength = 8; // Mínimo de 8 caracteres
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders(); // Provedores para tokens

// Configura autenticação com Google e JWT
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme; // JWT como padrão
    options.DefaultChallengeScheme = GoogleDefaults.AuthenticationScheme; // Google para desafios
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true, // Valida emissor
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidateAudience = true, // Valida audiência
        ValidAudience = builder.Configuration["Jwt:Audience"],
        ValidateIssuerSigningKey = true, // Valida chave
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"])
        ),
        ValidateLifetime = true, // Verifica expiração
        ClockSkew = TimeSpan.Zero // Sem tolerância
    };
})
.AddGoogle(options =>
{
    options.ClientId = builder.Configuration["Authentication:Google:ClientId"];
    options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"];
    options.SignInScheme = IdentityConstants.ExternalScheme; // Usa esquema externo do Identity
});

builder.Services.AddAuthorization(); // Habilita autorização
builder.Services.AddControllers(); // Adiciona suporte a controllers

var app = builder.Build();

app.UseHttpsRedirection();
app.UseAuthentication(); // Adiciona autenticação
app.UseAuthorization(); // Adiciona autorização
app.MapControllers();

app.Run();
```

**Explicação**:  
- Configura o Identity para gerenciar usuários.  
- Adiciona autenticação Google via OAuth, redirecionando para `signin-google`.  
- Configura JWT para proteger endpoints e gerar tokens após login.

### `ApplicationDbContext`

```csharp
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }
}
```

**Explicação**:  
Herda de `IdentityDbContext` para suportar tabelas do Identity.

---

## 4. 👤 Modelos e DTOs

### Modelo: `ApplicationUser`

```csharp
using Microsoft.AspNetCore.Identity;

public class ApplicationUser : IdentityUser
{
    public string NomeCompleto { get; set; } = string.Empty; // Nome completo
}
```

**Explicação**:  
Extende `IdentityUser` com campos personalizados.

### DTO: `RespuestaAutenticacionDTO`

```csharp
public class RespuestaAutenticacionDTO
{
    public string Token { get; set; } = null!; // JWT gerado
    public DateTime Expiracion { get; set; } // Expiração do JWT
}
```

**Explicação**:  
Retorna o JWT e sua expiração após login.

---

## 5. 🎮 AuthController (Login com Google)

Implementa endpoints para iniciar e completar o login com Google.

```csharp
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

// Controlador para login com Google
[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager; // Gerencia usuários
    private readonly SignInManager<ApplicationUser> _signInManager; // Gerencia login
    private readonly IConfiguration _configuration; // Acessa configurações

    public AuthController(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IConfiguration configuration)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _configuration = configuration;
    }

    // Inicia o login com Google
    [HttpGet("external-login/google")]
    [AllowAnonymous]
    public IActionResult ExternalLogin()
    {
        // Configura propriedades para o desafio OAuth
        var properties = _signInManager.ConfigureExternalAuthenticationProperties(
            "Google", // Provedor
            Url.Action("ExternalLoginCallback") // URL de callback
        );

        // Inicia o fluxo OAuth redirecionando ao Google
        return new ChallengeResult("Google", properties);
    }

    // Processa o callback do Google e gera JWT
    [HttpGet("external-login-callback")]
    [AllowAnonymous]
    public async Task<IActionResult> ExternalLoginCallback()
    {
        // Obtém informações do login externo
        var info = await _signInManager.GetExternalLoginInfoAsync();
        if (info == null)
            return BadRequest(new { Message = "Falha ao obter informações do Google." });

        // Tenta fazer login com o provedor externo
        var result = await _signInManager.ExternalLoginSignInAsync(
            info.LoginProvider,
            info.ProviderKey,
            isPersistent: false,
            bypassTwoFactor: true
        );

        // Se o usuário já está vinculado, gera JWT
        if (result.Succeeded)
        {
            var user = await _userManager.FindByLoginAsync(
                info.LoginProvider,
                info.ProviderKey
            );
            if (user == null)
                return NotFound(new { Message = "Usuário não encontrado." });

            var token = await GenerateJwtToken(user);
            return Ok(token);
        }

        // Se o usuário não existe, cria um novo
        var email = info.Principal.FindFirstValue(ClaimTypes.Email);
        if (string.IsNullOrEmpty(email))
            return BadRequest(new { Message = "E-mail não fornecido pelo Google." });

        var userName = email.Split('@')[0]; // Gera nome de usuário a partir do e-mail
        var user = await _userManager.FindByEmailAsync(email);

        if (user == null)
        {
            // Cria novo usuário
            user = new ApplicationUser
            {
                UserName = userName,
                Email = email,
                NomeCompleto = info.Principal.FindFirstValue(ClaimTypes.Name) ?? userName
            };

            var createResult = await _userManager.CreateAsync(user);
            if (!createResult.Succeeded)
                return BadRequest(new { Errors = createResult.Errors });

            // Vincula login do Google
            var addLoginResult = await _userManager.AddLoginAsync(
                user,
                new UserLoginInfo(
                    info.LoginProvider,
                    info.ProviderKey,
                    info.LoginProvider
                )
            );
            if (!addLoginResult.Succeeded)
                return BadRequest(new { Errors = addLoginResult.Errors });
        }

        // Gera JWT para o usuário
        var token = await GenerateJwtToken(user);
        return Ok(token);
    }

    // Gera JWT para o usuário
    private async Task<RespuestaAutenticacionDTO> GenerateJwtToken(ApplicationUser user)
    {
        // Define claims do JWT
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

        // Configura chave de assinatura
        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_configuration["Jwt:Key"])
        );
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expiry = DateTime.UtcNow.AddMinutes(30); // Expiração de 30 minutos

        // Cria JWT
        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: expiry,
            signingCredentials: creds
        );

        // Retorna DTO com JWT
        return new RespuestaAutenticacionDTO
        {
            Token = new JwtSecurityTokenHandler().WriteToken(token),
            Expiracion = expiry
        };
    }
}
```

**Explicação**:  
- `ExternalLogin`: Inicia o fluxo OAuth redirecionando ao Google.  
- `ExternalLoginCallback`: Processa o callback, cria ou vincula o usuário, e gera JWT.  
- `GenerateJwtToken`: Cria um JWT com claims do usuário.

---

## 6. 📌 Boas Práticas e Segurança

- **Validação de Credenciais**: Armazene `ClientId` e `ClientSecret` em variáveis de ambiente.  
- **HTTPS**: Use HTTPS para proteger o fluxo OAuth e JWTs.  
- **Erro de Callback**: Trate falhas no callback (ex.: `info == null`) com mensagens claras.  
- **Escopo do Google**: O código usa escopos padrão; adicione escopos específicos (ex.: `profile`) se necessário:
  ```csharp
  options.Scope.Add("profile");
  ```
- **Rate Limiting**: Limite requisições ao endpoint de login para evitar abuso.  
- **Logging**: Registre tentativas de login para auditoria.  
- **Testes**: Crie testes para simular o fluxo OAuth (ex.: mock do `ExternalLoginInfo`).  
- **JWT Seguro**: Use chaves fortes e tempos de expiração curtos (ex.: 30 minutos).  

---

## 7. 📋 Tabela de Endpoints

| Método | Endpoint                    | Descrição                              | Autenticação |
|--------|-----------------------------|----------------------------------------|--------------|
| GET    | `/api/auth/external-login/google` | Inicia login com Google           | Anônimo      |
| GET    | `/api/auth/external-login-callback` | Processa callback e gera JWT    | Anônimo      |



---

### Integração com Resumos Anteriores

Este resumo é compatível com seus resumos anteriores:
- **Autenticação (14/04/2025)**: Adicione o login com Google ao `AuthController` existente, mantendo endpoints como `login` e `refresh-token`. Use o mesmo `ApplicationUser` e configurações JWT.
- **Envio de E-mails (16/04/2025)**: Se desejar, combine com confirmação de e-mail, exigindo que usuários do Google confirmem o e-mail após o primeiro login (adicione `options.SignIn.RequireConfirmedEmail = true`).

Para integrar:
1. Inclua `Microsoft.AspNetCore.Authentication.Google` no projeto.
2. Adicione a configuração Google no `Program.cs` do resumo anterior.
3. Merge os métodos `ExternalLogin` e `ExternalLoginCallback` no `AuthController`, reusing `GenerateJwtToken`.

---

### Instruções para o GitHub

1. **Criar Arquivo**:
   - Copie o conteúdo do artefato.
   - Salve como `GoogleLoginGuide.md` em um editor (ex.: VS Code).

2. **Subir**:
   - No GitHub, use **Add file** > **Upload files** e faça upload.
   - Ou via Git:
     ```bash
     git add GoogleLoginGuide.md
     git commit -m "Adiciona guia de login com Google"
     git push origin main
     ```

3. **Verificar**:
   - Confirme que os blocos de código (C#, JSON, Bash) têm destaque de sintaxe.
   - Verifique os delimitadores (ex.: ```csharp) se houver problemas.

-