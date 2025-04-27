Claro! Vou fazer no mesmo padrão que o dos outros resumos: bem organizado, completo e focado apenas em **Refresh Tokens**.

---

# 📄 Tutorial: Implementando Refresh Tokens no ASP.NET Core 8 com JWT

## Índice
1. [Introdução](#introdução)
2. [Entendendo Refresh Token](#entendendo-refresh-token)
3. [Alterações na Configuração](#alterações-na-configuração)
4. [Atualizando o AuthService](#atualizando-o-authservice)
5. [Atualizando o AuthController](#atualizando-o-authcontroller)
6. [DTOs Usados](#dtos-usados)
7. [Explicação Técnica Final](#explicação-técnica-final)

---

## 1. Introdução

Aqui vamos aprender a adicionar **Refresh Token** ao nosso sistema de autenticação com **JWT** no ASP.NET Core 8.  
O objetivo é permitir que, quando o token de acesso expirar, o usuário possa solicitar um novo **sem precisar fazer login novamente**.

---

## 2. Entendendo Refresh Token

- **Access Token**: Token usado para acessar as APIs. Expira rápido (ex: 2h).
- **Refresh Token**: Token de longa duração (ex: 7 dias) usado **apenas** para obter novos Access Tokens.
- **Motivo**: Não expor o usuário a logins constantes e também não manter tokens de acesso eternos.

---

## 3. Alterações na Configuração

### 3.1 Atualizar o `appsettings.json`

Adicione configurações para tempo de expiração do Refresh Token:

```json
"JwtSettings": {
  "Issuer": "SeuProjetoAPI",
  "Audience": "SeuProjetoAPI",
  "SecretKey": "sua_chave_secreta_muito_forte_123456",
  "RefreshTokenExpirationDays": 7
}
```

- **RefreshTokenExpirationDays** define a validade dos Refresh Tokens.

---

## 4. Atualizando o AuthService

Agora o `AuthService` também vai gerar e gerenciar o Refresh Token.

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

    public async Task<AuthResultDto> GenerateAuthTokens(ApplicationUser user)
    {
        var accessToken = GenerateJwtToken(user);
        var refreshToken = GenerateRefreshToken();

        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(
            int.Parse(_configuration["JwtSettings:RefreshTokenExpirationDays"])
        );

        await _userManager.UpdateAsync(user);

        return new AuthResultDto
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken
        };
    }

    private string GenerateJwtToken(ApplicationUser user)
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

    private string GenerateRefreshToken()
    {
        var randomBytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes);
    }
}
```

### Importante:
- **Access Token**: Criado como antes.
- **Refresh Token**: Agora é uma string segura (64 bytes aleatórios).
- **Refresh Expiry**: Definido ao atualizar o usuário no banco.
- **Persistência**: O `ApplicationUser` precisa ter `RefreshToken` e `RefreshTokenExpiryTime` como propriedades.

---

## 5. Atualizando o AuthController

Adicione endpoint de **refresh** no `AuthController`:

```csharp
// Controllers/AuthController.cs
[HttpPost("refresh")]
public async Task<IActionResult> Refresh(TokenRequestDto request)
{
    var user = await _userManager.Users.FirstOrDefaultAsync(u => u.RefreshToken == request.RefreshToken);

    if (user == null || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
        return Unauthorized(new { Message = "Token inválido ou expirado." });

    var tokens = await _authService.GenerateAuthTokens(user);

    return Ok(tokens);
}
```

- **Busca pelo Refresh Token** no banco.
- Se não encontrar ou se o token estiver expirado, retorna **401 Unauthorized**.
- Se válido, gera novos tokens (Access + Refresh).

---

## 6. DTOs Usados

```csharp
// DTOs/AuthResultDto.cs
public class AuthResultDto
{
    public string AccessToken { get; set; }
    public string RefreshToken { get; set; }
}

// DTOs/TokenRequestDto.cs
public class TokenRequestDto
{
    public string RefreshToken { get; set; }
}
```

Esses DTOs padronizam a comunicação:

- **AuthResultDto**: Usado para responder com o novo Access Token + Refresh Token.
- **TokenRequestDto**: Usado para receber o Refresh Token no body da requisição.

---

## 7. Explicação Técnica Final

- **Primeiro Login** → Usuário recebe Access Token + Refresh Token.
- **Access Token expira** → Frontend chama `/refresh` com o Refresh Token.
- **Servidor valida o Refresh Token**:
  - Se válido, gera novos tokens.
  - Se inválido ou expirado, força login novamente.
- **Segurança**:
  - Tokens de acesso têm expiração curta para minimizar riscos.
  - Tokens de refresh também têm data de expiração e podem ser invalidados manualmente (ex: logout, alteração de senha).

---

# ✅ Aviso:

> Esse resumo foi extraído do seu material original, complementado com exemplos atualizados e melhores práticas, mas sem alterar seu conteúdo principal.

---

Quer que eu também já monte a versão em `.md` desse resumo para você usar direto no seu projeto? 🚀  
Se quiser, também posso montar agora a explicação **extra** sobre como fazer **refresh token rotativo** (onde cada refresh gera um novo refresh também)! Quer? 🎯
