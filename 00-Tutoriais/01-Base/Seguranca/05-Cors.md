

# 🌐 Configuração de CORS no ASP.NET Core 8

Este guia detalha como configurar **CORS (Cross-Origin Resource Sharing)** em uma **Web API ASP.NET Core 8** para permitir acesso seguro de frontends hospedados em domínios diferentes (ex.: Angular em `http://localhost:4200`). O código é comentado, integrável com autenticação JWT (dos resumos anteriores), e formatado para renderização correta no GitHub.

## 📘 Índice

1. O que é CORS?
2. Pacotes Necessários
3. Configuração Básica de CORS
4. Configuração Avançada com Políticas
5. Integração com Autenticação
6. Solução de Problemas Comuns
7. Boas Práticas e Segurança
8. Exemplo de Frontend (Opcional)

---

## 1. ❓ O que é CORS?

**CORS** é um mecanismo de segurança do navegador que controla quais origens (domínios) podem acessar recursos de uma API hospedada em outro domínio. Por exemplo:
- Sua API roda em `https://localhost:5001`.
- Seu frontend Angular roda em `http://localhost:4200`.
- Sem CORS, o navegador bloqueará requisições do frontend para a API devido à política de mesma origem (*same-origin policy*).

Este guia configura CORS para permitir essas requisições de forma segura.

---

## 2. 📦 Pacotes Necessários

Nenhum pacote adicional é necessário para CORS no ASP.NET Core 8, pois o suporte está embutido no framework. Se você está usando os resumos anteriores (autenticação, 2FA, etc.), já tem os pacotes relevantes:

```bash
dotnet add package Microsoft.AspNetCore.Identity.EntityFrameworkCore
dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer
```

**Explicação**:  
- CORS é configurado via middleware nativo do ASP.NET Core (`UseCors`).  
- Pacotes como `JwtBearer` são usados para proteger endpoints, e CORS deve permitir cabeçalhos como `Authorization`.

---

## 3. ⚙️ Configuração Básica de CORS

Configure CORS no `Program.cs` para permitir acesso de uma origem específica.

```csharp
var builder = WebApplication.CreateBuilder(args);

// Configura CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:4200") // Permite apenas o frontend Angular
              .AllowAnyMethod() // Permite GET, POST, etc.
              .AllowAnyHeader() // Permite cabeçalhos como Authorization
              .AllowCredentials(); // Permite cookies e credenciais (ex.: JWT)
    });
});

builder.Services.AddControllers();

var app = builder.Build();

app.UseHttpsRedirection();
app.UseCors("AllowFrontend"); // Aplica a política CORS
app.UseAuthentication(); // Se usar autenticação (ex.: JWT)
app.UseAuthorization();
app.MapControllers();

app.Run();
```

**Explicação**:  
- `AddCors`: Registra o serviço CORS com uma política chamada `AllowFrontend`.  
- `WithOrigins`: Especifica o domínio do frontend (ex.: `http://localhost:4200` para Angular).  
- `AllowAnyMethod`: Permite todos os métodos HTTP (GET, POST, PUT, etc.).  
- `AllowAnyHeader`: Permite cabeçalhos como `Authorization` (essencial para JWT).  
- `AllowCredentials`: Habilita envio de credenciais (ex.: cookies ou tokens).  
- `UseCors`: Aplica a política ao pipeline, antes de autenticação e autorização.

---

## 4. 🔧 Configuração Avançada com Políticas

Para maior controle, crie políticas específicas para diferentes cenários (ex.: endpoints públicos vs. protegidos).

```csharp
var builder = WebApplication.CreateBuilder(args);

// Configura múltiplas políticas CORS
builder.Services.AddCors(options =>
{
    // Política para endpoints públicos
    options.AddPolicy("AllowAnyOrigin", policy =>
    {
        policy.AllowAnyOrigin() // Qualquer origem pode acessar
              .AllowAnyMethod()
              .AllowAnyHeader();
    });

    // Política para endpoints autenticados
    options.AddPolicy("AllowFrontendAuth", policy =>
    {
        policy.WithOrigins(
                "http://localhost:4200", // Angular local
                "https://meufrontend.com" // Frontend em produção
              )
              .WithMethods("GET", "POST", "PUT") // Apenas métodos específicos
              .WithHeaders("Authorization", "Content-Type") // Apenas cabeçalhos necessários
              .AllowCredentials(); // Suporta JWT
    });
});

builder.Services.AddControllers();

var app = builder.Build();

app.UseHttpsRedirection();
app.UseCors(); // Aplica CORS globalmente (usará políticas específicas nos controllers)
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
```

### Aplicando Políticas nos Controllers

Use o atributo `[EnableCors]` para aplicar políticas específicas:

```csharp
[ApiController]
[Route("api/public")]
public class PublicController : ControllerBase
{
    // Endpoint público acessível por qualquer origem
    [HttpGet("info")]
    [EnableCors("AllowAnyOrigin")]
    public IActionResult GetInfo()
    {
        return Ok("Informação pública.");
    }
}

[ApiController]
[Route("api/protected")]
public class ProtectedController : ControllerBase
{
    // Endpoint protegido acessível apenas pelo frontend autorizado
    [HttpGet("data")]
    [Authorize]
    [EnableCors("AllowFrontendAuth")]
    public IActionResult GetData()
    {
        return Ok("Dados protegidos.");
    }
}
```

**Explicação**:  
- `AllowAnyOrigin`: Ideal para endpoints públicos, mas menos seguro.  
- `AllowFrontendAuth`: Restringe acesso a origens confiáveis, métodos e cabeçalhos, ideal para endpoints com autenticação JWT.  
- `[EnableCors]`: Aplica políticas específicas por controller ou endpoint.  
- `AllowCredentials` com `WithOrigins` exige origens explícitas (não funciona com `AllowAnyOrigin`).

---

## 5. 🔐 Integração com Autenticação

Para integrar com os resumos anteriores (ex.: autenticação JWT de 14/04/2025, 2FA de 16/04/2025), ajuste o `AuthController`:

```csharp
[ApiController]
[Route("api/auth")]
[EnableCors("AllowFrontendAuth")] // Aplica política autenticada
public class AuthController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IConfiguration _configuration;

    public AuthController(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IConfiguration configuration)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _configuration = configuration;
    }

    // Exemplo: Login com 2FA (do resumo anterior)
    [HttpPost("login-2fa")]
    [AllowAnonymous]
    public async Task<IActionResult> LoginWithTwoFactor(TwoFactorLoginDTO dto)
    {
        var user = await _userManager.FindByNameAsync(dto.UserName);
        if (user == null)
            return Unauthorized(new { Message = "Credenciais inválidas." });

        var passwordSignIn = await _signInManager.CheckPasswordSignInAsync(user, dto.Password, false);
        if (!passwordSignIn.Succeeded)
            return Unauthorized(new { Message = "Credenciais inválidas." });

        if (!await _userManager.GetTwoFactorEnabledAsync(user))
            return BadRequest(new { Message = "2FA não está ativado." });

        var isValidCode = await _userManager.VerifyTwoFactorTokenAsync(
            user,
            _userManager.Options.Tokens.AuthenticatorTokenProvider,
            dto.TwoFactorCode
        );
        if (!isValidCode)
            return Unauthorized(new { Message = "Código 2FA inválido." });

        var token = await GenerateJwtToken(user);
        return Ok(token);
    }

    // Método GenerateJwtToken (reutilizado do resumo anterior)
    private async Task<RespuestaAutenticacionDTO> GenerateJwtToken(ApplicationUser user)
    {
        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.UserName),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim("NomeCompleto", user.NomeCompleto),
            new Claim("Id", user.Id)
        };

        var userRoles = await _userManager.GetRolesAsync(user);
        claims.AddRange(userRoles.Select(role => new Claim(ClaimTypes.Role, role)));

        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_configuration["Jwt:Key"])
        );
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expiry = DateTime.UtcNow.AddMinutes(30);

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: expiry,
            signingCredentials: creds
        );

        return new RespuestaAutenticacionDTO
        {
            Token = new JwtSecurityTokenHandler().WriteToken(token),
            Expiracion = expiry
        };
    }
}
```

**Explicação**:  
- `[EnableCors("AllowFrontendAuth")]`: Garante que endpoints como `login-2fa` sejam acessíveis apenas pelo frontend autorizado (ex.: `http://localhost:4200`).  
- `AllowCredentials`: Permite enviar o cabeçalho `Authorization` com JWT.  
- Integra com 2FA, login com Google, e envio de e-mails, mantendo consistência.

---

## 6. 🛠️ Solução de Problemas Comuns

1. **Erro: "No 'Access-Control-Allow-Origin' header is present"**  
   - **Causa**: O middleware CORS não está configurado ou está após `UseRouting`.  
   - **Solução**: Coloque `app.UseCors()` antes de `app.UseAuthentication()` e `app.UseAuthorization()`:
     ```csharp
     app.UseCors("AllowFrontend");
     app.UseAuthentication();
     app.UseAuthorization();
     ```

2. **Erro: "Credentials flag is 'true', but the 'Access-Control-Allow-Credentials' header is not present"**  
   - **Causa**: `AllowCredentials()` não está na política, ou `AllowAnyOrigin` é usado com credenciais.  
   - **Solução**: Use `WithOrigins` explícitas e `AllowCredentials`:
     ```csharp
     policy.WithOrigins("http://localhost:4200")
           .AllowCredentials();
     ```

3. **Erro: "Method not allowed"**  
   - **Causa**: A política restringe métodos (ex.: `WithMethods("GET")` não inclui POST).  
   - **Solução**: Adicione métodos necessários:
     ```csharp
     policy.WithMethods("GET", "POST", "PUT");
     ```

4. **Erro no Frontend Angular**  
   - **Causa**: O frontend não envia credenciais corretamente.  
   - **Solução**: Configure o HttpClient para incluir credenciais:
     ```typescript
     // Angular: src/app/services/auth.service.ts
     import { HttpClient } from '@angular/common/http';
     import { Injectable } from '@angular/core';

     @Injectable({ providedIn: 'root' })
     export class AuthService {
       constructor(private http: HttpClient) {}

       login2fa(credentials: any) {
         return this.http.post('https://localhost:5001/api/auth/login-2fa', credentials, {
           withCredentials: true // Envia credenciais
         });
       }
     }
     ```

---

## 7. 📌 Boas Práticas e Segurança

- **Restringir Origens**: Use `WithOrigins` para listar apenas domínios confiáveis (ex.: `http://localhost:4200`, `https://meufrontend.com`). Evite `AllowAnyOrigin` em produção.  
- **Métodos Específicos**: Limite métodos com `WithMethods` para apenas os necessários (ex.: `GET`, `POST`).  
- **Cabeçalhos Controlados**: Use `WithHeaders` para permitir apenas cabeçalhos como `Authorization` e `Content-Type`.  
- **Credenciais Seguras**: Combine `AllowCredentials` com HTTPS para proteger tokens JWT.  
- **Pré-requisitos (Preflight)**: O ASP.NET Core lida automaticamente com requisições OPTIONS; garanta que `UseCors` esteja no início do pipeline.  
- **Logging**: Registre erros de CORS para depuração:
  ```csharp
  app.UseExceptionHandler(errorApp =>
  {
      errorApp.Run(async context =>
      {
          var exception = context.Features.Get<IExceptionHandlerFeature>();
          if (exception != null)
          {
              Console.WriteLine($"Erro CORS: {exception.Error.Message}");
              context.Response.StatusCode = 500;
              await context.Response.WriteAsync("Erro interno.");
          }
      });
  });
  ```
- **Testes**: Teste CORS com ferramentas como Postman ou cURL:
  ```bash
  curl -H "Origin: http://localhost:4200" \
       -H "Authorization: Bearer seu-token-jwt" \
       -X GET https://localhost:5001/api/protected/data
  ```
- **Produção**: Atualize `WithOrigins` com o domínio real do frontend em produção.

---

## 8. 🌟 Exemplo de Frontend (Opcional)

Se você usa Angular (como mencionado em 13/03/2025), aqui está um exemplo de configuração para consumir a API com CORS:

```typescript
// Angular: src/app/services/auth.service.ts
import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private apiUrl = 'https://localhost:5001/api/auth';

  constructor(private http: HttpClient) {}

  login2fa(credentials: { userName: string, password: string, twoFactorCode: string }): Observable<any> {
    return this.http.post(`${this.apiUrl}/login-2fa`, credentials, {
      withCredentials: true // Necessário para CORS com credenciais
    });
  }

  getProtectedData(): Observable<any> {
    return this.http.get('https://localhost:5001/api/protected/data', {
      withCredentials: true
    });
  }
}
```

**Explicação**:  
- `withCredentials: true`: Garante que o Angular envie cabeçalhos como `Authorization` e cookies.  
- Use o serviço em componentes para chamar endpoints protegidos.

---



---

### Integração com Resumos Anteriores

Este resumo é compatível com seus projetos anteriores:
- **Autenticação (14/04/2025)**: Adicione a configuração CORS ao `Program.cs` existente e aplique `[EnableCors]` ao `AuthController` para endpoints como `login` e `refresh-token`.
- **Envio de E-mails (16/04/2025)**: Endpoints como `register` e `forgot-password` se beneficiam da política `AllowFrontendAuth` para permitir acesso do frontend.
- **Login com Google (16/04/2025)**: O callback `external-login-callback` precisa de CORS para redirecionamentos do Google; use `AllowFrontendAuth`.
- **2FA (16/04/2025)**: O `TwoFactorController` e `login-2fa` já estão configurados com `[EnableCors("AllowFrontendAuth")]` acima.

Para integrar:
1. Adicione o middleware CORS ao `Program.cs` dos projetos anteriores:
   ```csharp
   builder.Services.AddCors(options =>
   {
       options.AddPolicy("AllowFrontendAuth", policy =>
       {
           policy.WithOrigins("http://localhost:4200")
                 .AllowAnyMethod()
                 .AllowAnyHeader()
                 .AllowCredentials();
       });
   });
   app.UseCors("AllowFrontendAuth");
   ```
2. Aplique `[EnableCors("AllowFrontendAuth")]` aos controllers existentes (ex.: `AuthController`, `TwoFactorController`).
3. Atualize o frontend (ex.: Angular) para incluir `withCredentials: true` nas requisições.

