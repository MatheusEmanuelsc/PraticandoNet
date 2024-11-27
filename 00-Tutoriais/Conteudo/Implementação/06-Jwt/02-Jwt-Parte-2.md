
---

### Parte 2: Implementa��o do `AuthController` com JWT e Pol�ticas de Autoriza��o

Neste resumo, vamos criar o controlador `AuthController`, respons�vel pela autentica��o, registro, refresh tokens, revoga��o de tokens e gerenciamento de roles (fun��es) utilizando pol�ticas de autoriza��o. Segue abaixo um passo a passo detalhado e comentado para cada uma dessas etapas.

#### �ndice
1. [Cria��o do Controlador `AuthController`](#1-cria��o-do-controlador-authcontroller)
2. [Endpoint de Login](#2-endpoint-de-login)
3. [Endpoint de Registro de Usu�rio](#3-endpoint-de-registro-de-usu�rio)
4. [Endpoint de Refresh Token](#4-endpoint-de-refresh-token)
5. [Endpoint de Revoga��o de Token](#5-endpoint-de-revoga��o-de-token)
6. [Gerenciamento de Roles](#6-gerenciamento-de-roles)
7. [Configura��o de Pol�ticas de Autoriza��o](#7-configura��o-de-pol�ticas-de-autoriza��o)

---

#### 1. Cria��o do Controlador `AuthController`

**Objetivo:** Configurar o controlador respons�vel por gerenciar a autentica��o e autoriza��o.

```csharp
using Curso.Api.Models;
using Curso.Api.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Curso.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ITokenService _tokenService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IConfiguration _configuration;

        public AuthController(ITokenService tokenService,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IConfiguration configuration)
        {
            _tokenService = tokenService;
            _userManager = userManager;
            _roleManager = roleManager;
            _configuration = configuration;
        }
    }
}
```

**Explica��o:**
- **`ITokenService`**: Servi�o respons�vel pela gera��o de tokens JWT.
- **`UserManager<ApplicationUser>`**: Gerencia as opera��es relacionadas aos usu�rios.
- **`RoleManager<IdentityRole>`**: Gerencia as opera��es relacionadas aos roles (fun��es) dos usu�rios.
- **`IConfiguration`**: Acessa as configura��es da aplica��o, como par�metros JWT.

---

#### 2. Endpoint de Login

**Objetivo:** Autenticar o usu�rio, gerar e retornar tokens JWT.

```csharp
[HttpPost]
[Route("login")]
public async Task<IActionResult> Login([FromBody] LoginModel model)
{
    var user = await _userManager.FindByNameAsync(model.Username!);

    if (user is not null && await _userManager.CheckPasswordAsync(user, model.Password!))
    {
        var userRoles = await _userManager.GetRolesAsync(user);
        var authClaims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, user.UserName!),
            new Claim(ClaimTypes.Email, user.Email!),
            new Claim("id", user.UserName!), // Adicionando o Claim "id"
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        };

        foreach (var userRole in userRoles)
        {
            authClaims.Add(new Claim(ClaimTypes.Role, userRole));
        }

        var token = _tokenService.GenerateAccessToken(authClaims, _configuration);
        var refreshToken = _tokenService.GenerateRefreshToken();

        _ = int.TryParse(_configuration["JWT:RefreshTokenValidityInMinutes"], out int refreshTokenValidityInMinutes);

        user.RefreshTokenExpiryTime = DateTime.Now.AddMinutes(refreshTokenValidityInMinutes);
        user.RefreshToken = refreshToken;

        await _userManager.UpdateAsync(user);

        return Ok(new
        {
            Token = new JwtSecurityTokenHandler().WriteToken(token),
            RefreshToken = refreshToken,
            Expiration = token.ValidTo
        });
    }

    return Unauthorized();
}
```

**Explica��o:**
- **`CheckPasswordAsync`**: Verifica se a senha informada � v�lida.
- **Claims**: S�o utilizados para guardar informa��es do usu�rio que estar�o presentes no token.
- **`GenerateAccessToken`**: M�todo do servi�o `ITokenService` para gerar o token de acesso.
- **`GenerateRefreshToken`**: Gera o refresh token, que permite renovar o token de acesso.

---

#### 3. Endpoint de Registro de Usu�rio

**Objetivo:** Registrar um novo usu�rio na aplica��o.

```csharp
[HttpPost]
[Route("register")]
public async Task<IActionResult> Register([FromBody] RegisterModel model)
{
    var userExists = await _userManager.FindByNameAsync(model.Username!);

    if (userExists != null)
    {
        return StatusCode(StatusCodes.Status500InternalServerError,
               new Response { Status = "Error", Message = "User already exists!" });
    }

    ApplicationUser user = new()
    {
        Email = model.Email,
        SecurityStamp = Guid.NewGuid().ToString(),
        UserName = model.Username
    };

    var result = await _userManager.CreateAsync(user, model.Password!);

    if (!result.Succeeded)
    {
        return StatusCode(StatusCodes.Status500InternalServerError,
               new Response { Status = "Error", Message = "User creation failed." });
    }

    return Ok(new Response { Status = "Success", Message = "User created successfully!" });
}
```

**Explica��o:**
- **`FindByNameAsync`**: Verifica se o usu�rio j� existe.
- **`CreateAsync`**: Cria o novo usu�rio e salva no banco de dados.

---

#### 4. Endpoint de Refresh Token

**Objetivo:** Renovar o token de acesso usando o refresh token.

```csharp
[HttpPost]
[Route("refresh-token")]
public async Task<IActionResult> RefreshToken(TokenModel tokenModel)
{
    if (tokenModel is null)
    {
        return BadRequest("Invalid client request");
    }

    string? accessToken = tokenModel.AccessToken ?? throw new ArgumentNullException(nameof(tokenModel));
    string? refreshToken = tokenModel.RefreshToken ?? throw new ArgumentException(nameof(tokenModel));

    var principal = _tokenService.GetPrincipalFromExpiredToken(accessToken!, _configuration);

    if (principal == null)
    {
        return BadRequest("Invalid access token/refresh token");
    }

    string username = principal.Identity.Name!;
    var user = await _userManager.FindByNameAsync(username);

    if (user == null || user.RefreshToken != refreshToken || user.RefreshTokenExpiryTime <= DateTime.Now)
    {
        return BadRequest("Invalid access token/refresh token");
    }

    var newAccessToken = _tokenService.GenerateAccessToken(principal.Claims.ToList(), _configuration);
    var newRefreshToken = _tokenService.GenerateRefreshToken();

    user.RefreshToken = newRefreshToken;
    await _userManager.UpdateAsync(user);

    return new ObjectResult(new
    {
        accessToken = new JwtSecurityTokenHandler().WriteToken(newAccessToken),
        refreshToken = newRefreshToken
    });
}
```

**Explica��o:**
- **`GetPrincipalFromExpiredToken`**: Recupera as informa��es do usu�rio do token expirado.
- **Renova��o do Token**: Cria novos tokens e atualiza os dados do usu�rio no banco.

---

#### 5. Endpoint de Revoga��o de Token

**Objetivo:** Revogar o refresh token de um usu�rio espec�fico.

```csharp
[Authorize]
[HttpPost]
[Route("revoke/{username}")]
public async Task<IActionResult> Revoke(string username)
{
    var user = await _userManager.FindByNameAsync(username);

    if (user == null) return BadRequest("Invalid user name");

    user.RefreshToken = null;
    await _userManager.UpdateAsync(user);

    return NoContent();
}
```

**Explica��o:**
- **Revoga��o**: Anula o refresh token do usu�rio, impedindo a renova��o do token de acesso.

---

#### 6. Gerenciamento de Roles

**Objetivo:** Criar novas roles e atribuir usu�rios a roles espec�ficas.

**Criar Role:**
```csharp
[HttpPost]
[Route("CreateRole")]
public async Task<IActionResult> CreateRole(string roleName)
{
    var roleExist = await _roleManager.RoleExistsAsync(roleName);

    if (!roleExist)
    {
        var roleResult = await _roleManager.CreateAsync(new IdentityRole(roleName));

        if (roleResult.Succeeded)
        {
            _logger.LogInformation(1, "Roles Added");
            return StatusCode(StatusCodes.Status200OK,
                    new Response
                    {
                        Status = "Success",
                        Message = $"Role {roleName} added successfully"
                    });
        }
        else
        {
            _logger.LogInformation(2, "Error");
            return StatusCode(StatusCodes.Status400BadRequest,
               new Response
               {
                   Status = "Error",
                   Message = $"Issue adding the new {roleName} role"
               });
        }
    }
    return StatusCode(StatusCodes.Status400BadRequest,
      new Response { Status = "Error", Message = "Role already exist." });
}
```

**Adicionar Usu�rio a uma Role:**
```csharp
[HttpPost]
[Route("AddUserToRole")]
public async Task<IActionResult> AddUserToRole(string email, string roleName)
{
    var user = await _userManager.FindByEmailAsync(email);



    if (user != null)
    {
        var result = await _userManager.AddToRoleAsync(user, roleName);

        if (result.Succeeded)
        {
            return StatusCode(StatusCodes.Status200OK,
              new Response { Status = "Success", Message = "User added to the role successfully!" });
        }
    }

    return StatusCode(StatusCodes.Status400BadRequest,
          new Response { Status = "Error", Message = "User not added to the role!" });
}
```

**Explica��o:**
- **`CreateRole`**: Cria uma nova role no sistema.
- **`AddUserToRole`**: Atribui um usu�rio existente a uma role espec�fica.

---

#### 7. Configura��o de Pol�ticas de Autoriza��o

**Objetivo:** Definir pol�ticas de autoriza��o baseadas em roles.

```csharp
services.AddAuthorization(options =>
{
    options.AddPolicy("RequireAdministratorRole",
         policy => policy.RequireRole("Administrator"));
    options.AddPolicy("RequireUserRole",
         policy => policy.RequireRole("User"));
});
```

**Explica��o:**
- **Pol�tica de Administrador:** Apenas usu�rios com a role "Administrator" t�m acesso.
- **Pol�tica de Usu�rio:** Apenas usu�rios com a role "User" t�m acesso.

---

### Conclus�o

Este resumo detalha a cria��o do controlador `AuthController` para gerenciar autentica��o, gera��o de tokens JWT, e gerenciamento de roles utilizando pol�ticas de autoriza��o. Cada etapa foi explicada para garantir uma compreens�o completa da implementa��o e como utilizar esses conceitos em uma aplica��o ASP.NET Core.

---
