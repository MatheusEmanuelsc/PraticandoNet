
---

### Parte 2: Implementação do `AuthController` com JWT e Políticas de Autorização

Neste resumo, vamos criar o controlador `AuthController`, responsável pela autenticação, registro, refresh tokens, revogação de tokens e gerenciamento de roles (funções) utilizando políticas de autorização. Segue abaixo um passo a passo detalhado e comentado para cada uma dessas etapas.

#### Índice
1. [Criação do Controlador `AuthController`](#1-criação-do-controlador-authcontroller)
2. [Endpoint de Login](#2-endpoint-de-login)
3. [Endpoint de Registro de Usuário](#3-endpoint-de-registro-de-usuário)
4. [Endpoint de Refresh Token](#4-endpoint-de-refresh-token)
5. [Endpoint de Revogação de Token](#5-endpoint-de-revogação-de-token)
6. [Gerenciamento de Roles](#6-gerenciamento-de-roles)
7. [Configuração de Políticas de Autorização](#7-configuração-de-políticas-de-autorização)

---

#### 1. Criação do Controlador `AuthController`

**Objetivo:** Configurar o controlador responsável por gerenciar a autenticação e autorização.

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

**Explicação:**
- **`ITokenService`**: Serviço responsável pela geração de tokens JWT.
- **`UserManager<ApplicationUser>`**: Gerencia as operações relacionadas aos usuários.
- **`RoleManager<IdentityRole>`**: Gerencia as operações relacionadas aos roles (funções) dos usuários.
- **`IConfiguration`**: Acessa as configurações da aplicação, como parâmetros JWT.

---

#### 2. Endpoint de Login

**Objetivo:** Autenticar o usuário, gerar e retornar tokens JWT.

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

**Explicação:**
- **`CheckPasswordAsync`**: Verifica se a senha informada é válida.
- **Claims**: São utilizados para guardar informações do usuário que estarão presentes no token.
- **`GenerateAccessToken`**: Método do serviço `ITokenService` para gerar o token de acesso.
- **`GenerateRefreshToken`**: Gera o refresh token, que permite renovar o token de acesso.

---

#### 3. Endpoint de Registro de Usuário

**Objetivo:** Registrar um novo usuário na aplicação.

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

**Explicação:**
- **`FindByNameAsync`**: Verifica se o usuário já existe.
- **`CreateAsync`**: Cria o novo usuário e salva no banco de dados.

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

**Explicação:**
- **`GetPrincipalFromExpiredToken`**: Recupera as informações do usuário do token expirado.
- **Renovação do Token**: Cria novos tokens e atualiza os dados do usuário no banco.

---

#### 5. Endpoint de Revogação de Token

**Objetivo:** Revogar o refresh token de um usuário específico.

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

**Explicação:**
- **Revogação**: Anula o refresh token do usuário, impedindo a renovação do token de acesso.

---

#### 6. Gerenciamento de Roles

**Objetivo:** Criar novas roles e atribuir usuários a roles específicas.

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

**Adicionar Usuário a uma Role:**
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

**Explicação:**
- **`CreateRole`**: Cria uma nova role no sistema.
- **`AddUserToRole`**: Atribui um usuário existente a uma role específica.

---

#### 7. Configuração de Políticas de Autorização

**Objetivo:** Definir políticas de autorização baseadas em roles.

```csharp
services.AddAuthorization(options =>
{
    options.AddPolicy("RequireAdministratorRole",
         policy => policy.RequireRole("Administrator"));
    options.AddPolicy("RequireUserRole",
         policy => policy.RequireRole("User"));
});
```

**Explicação:**
- **Política de Administrador:** Apenas usuários com a role "Administrator" têm acesso.
- **Política de Usuário:** Apenas usuários com a role "User" têm acesso.

---

### Conclusão

Este resumo detalha a criação do controlador `AuthController` para gerenciar autenticação, geração de tokens JWT, e gerenciamento de roles utilizando políticas de autorização. Cada etapa foi explicada para garantir uma compreensão completa da implementação e como utilizar esses conceitos em uma aplicação ASP.NET Core.

---
