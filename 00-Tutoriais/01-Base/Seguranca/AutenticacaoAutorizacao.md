

```md
# 🔐 Autenticação e Autorização com ASP.NET Core 8 (Identity + JWT)

Este tutorial completo mostra como implementar um sistema de autenticação e autorização usando ASP.NET Core 8 com:

- ✅ ASP.NET Core Identity
- ✅ JWT Token + Refresh Token
- ✅ Confirmação de E-mail
- ✅ Recuperação de Senha
- ✅ Autorização por Roles e Claims
- ✅ Proteção de Rotas (Policy & Role-based)

---

## 📘 Índice

1. [1. Pacotes e Estrutura Inicial](#1-pacotes-e-estrutura-inicial)
2. [2. Configurar Identity no Program.cs](#2-configurar-identity-no-programcs)
3. [3. Models: Usuário e RefreshToken](#3-models-usuário-e-refreshtoken)
4. [4. Registro de Usuário + Confirmação de E-mail](#4-registro-de-usuário--confirmação-de-e-mail)
5. [5. Login com JWT + Refresh Token](#5-login-com-jwt--refresh-token)
6. [6. Endpoint de Refresh Token](#6-endpoint-de-refresh-token)
7. [7. Esqueci Minha Senha e Redefinição](#7-esquecei-minha-senha-e-redefinição)
8. [8. Autorização com Roles e Claims](#8-autorização-com-roles-e-claims)
9. [9. Protegendo Rotas com [Authorize]](#9-protegendo-rotas-com-authorize)
10. [10. Considerações Finais](#10-considerações-finais)

---

## 1. 📦 Pacotes e Estrutura Inicial

Instale os pacotes:

```bash
dotnet add package Microsoft.AspNetCore.Identity.EntityFrameworkCore
dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer
dotnet add package Microsoft.EntityFrameworkCore.SqlServer
```

---

## 2. ⚙️ Configurar Identity no `Program.cs`

```csharp
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.SignIn.RequireConfirmedEmail = true;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var key = builder.Configuration["Jwt:Key"]!;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
            ValidateIssuer = false,
            ValidateAudience = false
        };
    });

builder.Services.AddAuthorization();
```

---

## 3. 🧱 Models: `ApplicationUser` e `RefreshToken`

```csharp
public class ApplicationUser : IdentityUser
{
    // Campos personalizados, se quiser
}
```

```csharp
public class RefreshToken
{
    public int Id { get; set; }
    public string Token { get; set; } = string.Empty;
    public DateTime Expiration { get; set; }
    public string UserId { get; set; } = string.Empty;
    public ApplicationUser? User { get; set; }
    public bool Revoked { get; set; } = false;
}
```

No `ApplicationDbContext`:

```csharp
public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
```

---

## 4. 📝 Registro de Usuário + Confirmação de E-mail

```csharp
[HttpPost("register")]
public async Task<IActionResult> Register(RegisterDto dto)
{
    var user = new ApplicationUser { UserName = dto.Username, Email = dto.Email };
    var result = await _userManager.CreateAsync(user, dto.Password);

    if (!result.Succeeded)
        return BadRequest(result.Errors);

    await _userManager.AddToRoleAsync(user, "User");

    var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
    var link = Url.Action(nameof(ConfirmEmail), "Auth", new { userId = user.Id, token }, Request.Scheme);
    await _emailService.SendAsync(user.Email!, "Confirme seu e-mail", link!);

    return Ok("Usuário criado. Verifique seu e-mail.");
}
```

```csharp
[HttpGet("confirm")]
public async Task<IActionResult> ConfirmEmail(string userId, string token)
{
    var user = await _userManager.FindByIdAsync(userId);
    var result = await _userManager.ConfirmEmailAsync(user!, token);
    return result.Succeeded ? Ok("E-mail confirmado!") : BadRequest("Erro");
}
```

---

## 5. 🔐 Login com JWT + Refresh Token

```csharp
[HttpPost("login")]
public async Task<IActionResult> Login(LoginDto dto)
{
    var user = await _userManager.FindByNameAsync(dto.Username);
    if (user == null || !await _userManager.CheckPasswordAsync(user, dto.Password))
        return Unauthorized("Credenciais inválidas");

    var roles = await _userManager.GetRolesAsync(user);
    var token = _tokenService.GenerateToken(user, roles);
    var refresh = new RefreshToken
    {
        Token = Guid.NewGuid().ToString(),
        Expiration = DateTime.UtcNow.AddDays(7),
        UserId = user.Id
    };

    await _context.RefreshTokens.AddAsync(refresh);
    await _context.SaveChangesAsync();

    return Ok(new { token, refreshToken = refresh.Token });
}
```

---

## 6. 🔁 Endpoint de Refresh Token

```csharp
[HttpPost("refresh")]
public async Task<IActionResult> RefreshToken([FromBody] string token)
{
    var old = await _context.RefreshTokens.Include(x => x.User)
        .FirstOrDefaultAsync(x => x.Token == token && !x.Revoked && x.Expiration > DateTime.UtcNow);

    if (old is null) return Unauthorized("Inválido ou expirado");

    old.Revoked = true;

    var roles = await _userManager.GetRolesAsync(old.User!);
    var newToken = _tokenService.GenerateToken(old.User!, roles);

    var refresh = new RefreshToken
    {
        Token = Guid.NewGuid().ToString(),
        Expiration = DateTime.UtcNow.AddDays(7),
        UserId = old.UserId
    };

    _context.RefreshTokens.Add(refresh);
    await _context.SaveChangesAsync();

    return Ok(new { token = newToken, refreshToken = refresh.Token });
}
```

---

## 7. 🔧 Esqueci Minha Senha e Redefinição

### DTOs

```csharp
public class ForgotPasswordDto { public string Email { get; set; } = ""; }
public class ResetPasswordDto
{
    public string Email { get; set; } = "";
    public string Token { get; set; } = "";
    public string NewPassword { get; set; } = "";
}
```

### Endpoints

```csharp
[HttpPost("forgot")]
public async Task<IActionResult> ForgotPassword(ForgotPasswordDto dto)
{
    var user = await _userManager.FindByEmailAsync(dto.Email);
    if (user == null) return Ok(); // segurança

    var token = await _userManager.GeneratePasswordResetTokenAsync(user);
    var link = Url.Action(nameof(ResetPassword), "Auth", new { token, email = user.Email }, Request.Scheme);
    await _emailService.SendAsync(user.Email!, "Redefinir senha", link!);
    return Ok("Verifique seu e-mail.");
}

[HttpPost("reset")]
public async Task<IActionResult> ResetPassword(ResetPasswordDto dto)
{
    var user = await _userManager.FindByEmailAsync(dto.Email);
    if (user == null) return BadRequest("Inválido");

    var result = await _userManager.ResetPasswordAsync(user, dto.Token, dto.NewPassword);
    return result.Succeeded ? Ok("Senha atualizada") : BadRequest("Erro");
}
```

---

## 8. 🔐 Autorização com Roles e Claims

### Criar Roles

```csharp
var roleManager = app.Services.GetRequiredService<RoleManager<IdentityRole>>();
await roleManager.CreateAsync(new IdentityRole("Admin"));
await roleManager.CreateAsync(new IdentityRole("User"));
```

### Atribuir Role

```csharp
await _userManager.AddToRoleAsync(user, "Admin");
```

---

## 9. 🛡️ Protegendo Rotas com `[Authorize]`

```csharp
[Authorize]
[HttpGet("me")]
public IActionResult GetProfile() => Ok(User.Identity?.Name);
```

```csharp
[Authorize(Roles = "Admin")]
[HttpGet("admin")]
public IActionResult AdminOnly() => Ok("Acesso de administrador");
```

---

## 10. 🧾 Considerações Finais

- Use HTTPS em produção
- Configure `SendGrid` ou `SMTP` real no lugar do mock de e-mail
- Adicione tempo de expiração ao JWT
- Use Redis ou banco para revogar tokens, se necessário
- Tokens devem ter `HttpOnly` em cookies ou ser rotacionados com frequência
- Use policies para claims complexas

