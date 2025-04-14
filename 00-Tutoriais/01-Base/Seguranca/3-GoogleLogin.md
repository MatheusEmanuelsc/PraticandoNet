

# 🔗 Login com Conta do Google no ASP.NET Core 8

Este tutorial explica como implementar a autenticação externa com uma conta do Google no **ASP.NET Core 8**, integrado a um sistema de autenticação baseado em **Identity** e **JWT**. Após o login com o Google, o sistema criará um usuário no banco de dados (se necessário) e retornará um **JWT** para autenticação nas APIs, mantendo a consistência com o fluxo existente.

---

## 📘 Índice

1. [Pré-requisitos](#1-pré-requisitos)
2. [Configuração do Google OAuth no Console do Google](#2-configuração-do-google-oauth-no-console-do-google)
3. [Configuração no ASP.NET Core](#3-configuração-no-aspnet-core)
4. [Ajustes no Model e DTOs](#4-ajustes-no-model-e-dtos)
5. [Atualização do Controller de Usuários](#5-atualização-do-controller-de-usuários)
6. [Frontend (Considerações)](#6-frontend-considerações)
7. [Boas Práticas e Considerações](#7-boas-práticas-e-considerações)

---

## 1. ❓ Pré-requisitos

- **Projeto Existente**: Um projeto ASP.NET Core 8 configurado com **Identity** e **JWT**, conforme descrito nos tutoriais anteriores.
- **Google Developer Console**: Uma conta no Google para criar credenciais OAuth.
- **Pacotes**: Já temos os pacotes necessários (`Microsoft.AspNetCore.Identity.EntityFrameworkCore`, `Microsoft.AspNetCore.Authentication.JwtBearer`), mas adicionaremos autenticação externa.

Adicione o pacote para autenticação com Google:

```bash
dotnet add package Microsoft.AspNetCore.Authentication.Google
```

---

## 2. 🔑 Configuração do Google OAuth no Console do Google

1. **Acesse o Google Cloud Console**:
   - Vá para [console.developers.google.com](https://console.developers.google.com).
   - Crie um novo projeto (ou selecione um existente).

2. **Habilitar APIs**:
   - No menu, vá para **APIs & Services** > **Library**.
   - Habilite a **Google+ API** ou **People API** (necessária para obter informações do usuário).

3. **Criar Credenciais OAuth**:
   - Vá para **APIs & Services** > **Credentials**.
   - Clique em **Create Credentials** > **OAuth 2.0 Client IDs**.
   - Escolha o tipo **Web application**.
   - Configure:
     - **Authorized JavaScript origins**: Adicione o domínio da sua aplicação (ex.: `https://localhost:5001` para desenvolvimento).
     - **Authorized redirect URIs**: Adicione a URL de callback da sua aplicação (ex.: `https://localhost:5001/signin-google`).
   - Após criar, anote o **Client ID** e **Client Secret**.

4. **Salvar Credenciais**:
   - Adicione o **Client ID** e **Client Secret** ao `appsettings.json` do seu projeto.

---

## 3. ⚙️ Configuração no ASP.NET Core

### Configurar no `appsettings.json`

Adicione as credenciais do Google ao `appsettings.json`:

```json
{
  "Google": {
    "ClientId": "seu-client-id.apps.googleusercontent.com",
    "ClientSecret": "seu-client-secret"
  }
}
```

> **Nota**: Em produção, armazene essas credenciais em um gerenciador de segredos (como **Azure Key Vault** ou variáveis de ambiente) para maior segurança.

### Configurar Autenticação no `Program.cs`

Atualize o `Program.cs` para incluir a autenticação com Google:

```csharp
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
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidateAudience = true,
        ValidAudience = builder.Configuration["Jwt:Audience"],
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"])),
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
})
.AddGoogle(options =>
{
    options.ClientId = builder.Configuration["Google:ClientId"];
    options.ClientSecret = builder.Configuration["Google:ClientSecret"];
    options.SignInScheme = IdentityConstants.ExternalScheme;
});

builder.Services.AddAuthorization();
```

### Configurar Identity

Certifique-se de que o **Identity** já está configurado (como no tutorial anterior):

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

## 4. 👤 Ajustes no Model e DTOs

### Model `ApplicationUser`

O model `ApplicationUser` já está configurado, mas podemos adicionar um campo opcional para rastrear logins externos:

```csharp
public class ApplicationUser : IdentityUser
{
    public string NomeCompleto { get; set; } = string.Empty;
    public string? ExternalProvider { get; set; } // Ex.: "Google"
    public string? ExternalId { get; set; } // ID do provedor externo
}
```

### DTOs

Os DTOs existentes (`RespuestaAutenticacionDTO`) são suficientes, mas podemos criar um DTO para o callback do Google, se necessário:

```csharp
public class ExternalLoginDTO
{
    public string Provider { get; set; } = null!;
    public string Token { get; set; } = null!;
}
```

---

## 5. 🎮 Atualização do Controller de Usuários

Atualize o controller para suportar o login com Google. O fluxo será:

1. Redirecionar o usuário para a página de login do Google.
2. Após o login, o Google redireciona de volta para a aplicação.
3. Criar ou vincular o usuário no banco e retornar um **JWT**.

### Controller Atualizado

```csharp
using Microsoft.AspNetCore.Authentication;
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
        private readonly IEmailService _emailService;

        public UsuariosController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IConfiguration configuration,
            RoleManager<IdentityRole> roleManager,
            IEmailService emailService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
            _roleManager = roleManager;
            _emailService = emailService;
        }

        // Endpoint para iniciar o login com Google
        [HttpGet("login-google")]
        [AllowAnonymous]
        public IActionResult LoginGoogle()
        {
            var redirectUrl = Url.Action("GoogleCallback", "Usuarios", null, Request.Scheme);
            var properties = _signInManager.ConfigureExternalAuthenticationProperties("Google", redirectUrl);
            return new ChallengeResult("Google", properties);
        }

        // Callback do Google
        [HttpGet("google-callback")]
        [AllowAnonymous]
        public async Task<IActionResult> GoogleCallback()
        {
            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
                return BadRequest(new { Message = "Erro ao obter informações do Google" });

            // Tenta fazer login com o provedor externo
            var result = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false, bypassTwoFactor: true);
            if (result.Succeeded)
            {
                // Usuário já existe, gerar JWT
                var user = await _userManager.FindByLoginAsync(info.LoginProvider, info.ProviderKey);
                var token = await GenerateJwtToken(user);
                return Ok(token);
            }

            // Usuário não existe, criar novo
            var email = info.Principal.FindFirstValue(ClaimTypes.Email);
            var name = info.Principal.FindFirstValue(ClaimTypes.Name);

            if (string.IsNullOrEmpty(email))
                return BadRequest(new { Message = "E-mail não fornecido pelo Google" });

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                user = new ApplicationUser
                {
                    UserName = email,
                    Email = email,
                    NomeCompleto = name ?? email,
                    ExternalProvider = info.LoginProvider,
                    ExternalId = info.ProviderKey,
                    EmailConfirmed = true // Google já valida o e-mail
                };

                var createResult = await _userManager.CreateAsync(user);
                if (!createResult.Succeeded)
                    return BadRequest(createResult.Errors);

                await _userManager.AddLoginAsync(user, info);
            }

            // Gerar JWT para o usuário
            var token = await GenerateJwtToken(user);
            return Ok(token);
        }

        // Métodos existentes (registro, login, etc.) permanecem como no tutorial anterior
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

        // Outros métodos (registro, esqueci-senha, etc.) como nos tutoriais anteriores
    }
}
```

---

## 6. 🌐 Frontend (Considerações)

Embora este tutorial foque no backend, o frontend precisa iniciar o fluxo de login com Google. Algumas opções:

- **Botão de Login com Google**:
  - Use a biblioteca do Google Sign-In (`gapi`) ou redirecione diretamente para o endpoint `/api/usuarios/login-google`.
  - Exemplo de botão em HTML/JS:
    ```html
    <a href="/api/usuarios/login-google" class="btn btn-google">Login com Google</a>
    ```

- **Redirecionamento Após Callback**:
  - Após o callback bem-sucedido (`google-callback`), o backend retorna o JWT. O frontend deve capturar o token (por exemplo, via redirecionamento com query string ou armazenando em cookies seguros) e usá-lo para autenticar requisições.

- **SPA (React, Angular, Vue)**:
  - Chame o endpoint `/api/usuarios/login-google` via fetch ou axios, redirecione o usuário para a URL retornada e processe o JWT no callback.

---

## 7. 📌 Boas Práticas e Considerações

- **Segurança das Credenciais**: Nunca exponha o **Client Secret** no frontend. Armazene-o apenas no backend ou em gerenciadores de segredos.
- **Validação de E-mail**: O Google já valida o e-mail, então você pode marcar `EmailConfirmed = true` para novos usuários.
- **Vinculação de Contas**: Permita que usuários existentes vinculem suas contas Google a contas locais usando `UserManager.AddLoginAsync`.
- **Erro de Redirecionamento**: Configure corretamente os URIs de callback no Google Console para evitar erros como `redirect_uri_mismatch`.
- **Refresh Tokens**: Integre refresh tokens para logins externos, como no fluxo principal.
- **Logs**: Registre falhas de autenticação externa para monitoramento.
- **Testes**: Use o ambiente de desenvolvimento do Google Console para testar sem afetar produção.

---

Este tutorial adiciona o login com Google ao sistema de autenticação existente, mantendo a consistência com o uso de **JWT** e **Identity**. Você pode expandir para outros provedores (como Facebook ou Microsoft) seguindo um fluxo semelhante, apenas alterando o provedor na configuração.

