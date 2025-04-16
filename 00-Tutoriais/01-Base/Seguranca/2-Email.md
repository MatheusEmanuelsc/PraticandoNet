

```markdown
# 📧 Envio de E-mails para Confirmação e Recuperação de Senha no ASP.NET Core 8

Este guia detalha como implementar o **envio de e-mails** para **confirmação de conta** e **recuperação de senha** em uma aplicação **ASP.NET Core 8** usando o **SendGrid**. Inclui configurações, integração com o Identity, e um controller ajustado para enviar e-mails com links clicáveis. O código é comentado para clareza e formatado para renderização correta no GitHub.

## 📘 Índice

1. Pacotes Necessários
2. Configuração do SendGrid
3. Ajustes no Identity
4. Modelos e DTOs
5. EmailService
6. AuthController (Apenas E-mail)
7. Boas Práticas e Segurança
8. Tabela de Endpoints

---

## 1. 📦 Pacotes Necessários

Adicione os pacotes para Identity, Entity Framework e SendGrid via NuGet:

```bash
dotnet add package Microsoft.AspNetCore.Identity.EntityFrameworkCore
dotnet add package Microsoft.EntityFrameworkCore.SqlServer
dotnet add package SendGrid
```

**Explicação**:  
- `Microsoft.AspNetCore.Identity.EntityFrameworkCore`: Gerencia usuários e tokens do Identity.  
- `Microsoft.EntityFrameworkCore.SqlServer`: Persiste dados do Identity no SQL Server.  
- `SendGrid`: Biblioteca para envio de e-mails via API do SendGrid.

---

## 2. 📬 Configuração do SendGrid

### Obter Chave API
1. Crie uma conta no [SendGrid](https://sendgrid.com/).
2. Gere uma **API Key** no painel (Settings > API Keys > Create API Key).
3. Copie a chave para uso na aplicação.

### `appsettings.json`

Adicione as configurações do SendGrid e do remetente do e-mail:

```json
{
  "SendGrid": {
    "ApiKey": "SUA-CHAVE-API-SENDGRID", // Chave API do SendGrid
    "FromEmail": "no-reply@suaapi.com", // E-mail remetente
    "FromName": "Sua API" // Nome do remetente
  }
}
```

**Explicação**:  
- `ApiKey`: Autentica requisições ao SendGrid.  
- `FromEmail` e `FromName`: Definem o remetente dos e-mails enviados.  
Armazene a chave em variáveis de ambiente em produção para maior segurança.

---

## 3. ⚙️ Ajustes no Identity

Configure o Identity no `Program.cs` para suportar confirmação de e-mail:

```csharp
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.SignIn.RequireConfirmedEmail = true; // Exige confirmação de e-mail antes do login
    options.Password.RequireDigit = true; // Senha deve conter dígito
    options.Password.RequiredLength = 8; // Mínimo de 8 caracteres
})
.AddEntityFrameworkStores<ApplicationDbContext>() // Usa EF com o contexto
.AddDefaultTokenProviders(); // Provedores para tokens de e-mail e senha
```

**Explicação**:  
- `RequireConfirmedEmail = true`: Garante que usuários só façam login após confirmar o e-mail.  
- `AddDefaultTokenProviders()`: Habilita geração de tokens para confirmação de e-mail e redefinição de senha.

---

## 4. 👤 Modelos e DTOs

### Modelo: `ApplicationUser`

```csharp
public class ApplicationUser : IdentityUser
{
    public string NomeCompleto { get; set; } = string.Empty; // Nome completo do usuário
}
```

**Explicação**:  
Herda de `IdentityUser` para incluir propriedades padrão (ex.: `Email`, `UserName`) e adiciona `NomeCompleto`.

### DTOs

#### `RegisterDTO`:

```csharp
public class RegisterDTO
{
    [Required]
    public string UserName { get; set; } = null!; // Nome de usuário
    [Required, EmailAddress]
    public string Email { get; set; } = null!; // E-mail válido
    [Required, MinLength(8)]
    public string Password { get; set; } = null!; // Senha
    [Required]
    public string NomeCompleto { get; set; } = null!; // Nome completo
}
```

**Explicação**:  
Recebe dados de registro com validações.

#### `ResetPasswordDTO`:

```csharp
public class ResetPasswordDTO
{
    [Required, EmailAddress]
    public string Email { get; set; } = null!; // E-mail do usuário
    [Required]
    public string Token { get; set; } = null!; // Token de redefinição
    [Required, MinLength(8)]
    public string NovaSenha { get; set; } = null!; // Nova senha
}
```

**Explicação**:  
Recebe dados para redefinir a senha.

---

## 5. 📧 EmailService

Crie um serviço para gerenciar o envio de e-mails com SendGrid.

```csharp
using Microsoft.Extensions.Configuration;
using SendGrid;
using SendGrid.Helpers.Mail;
using System.Threading.Tasks;

public interface IEmailService
{
    Task SendEmailAsync(string toEmail, string subject, string htmlContent); // Envia e-mail HTML
}

public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration; // Acessa configurações

    public EmailService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    // Envia e-mail usando SendGrid
    public async Task SendEmailAsync(string toEmail, string subject, string htmlContent)
    {
        var apiKey = _configuration["SendGrid:ApiKey"]; // Obtém chave API
        var client = new SendGridClient(apiKey); // Inicializa cliente SendGrid
        var from = new EmailAddress(
            _configuration["SendGrid:FromEmail"], // E-mail remetente
            _configuration["SendGrid:FromName"] // Nome remetente
        );
        var to = new EmailAddress(toEmail); // Destinatário
        var msg = MailHelper.CreateSingleEmail(
            from,
            to,
            subject,
            null, // Sem conteúdo de texto puro
            htmlContent // Conteúdo HTML
        );
        var response = await client.SendEmailAsync(msg); // Envia e-mail

        // Verifica se o envio falhou
        if (response.StatusCode != System.Net.HttpStatusCode.OK &&
            response.StatusCode != System.Net.HttpStatusCode.Accepted)
        {
            throw new Exception($"Falha ao enviar e-mail: {response.StatusCode}");
        }
    }
}
```

**Explicação**:  
- `IEmailService`: Interface para envio de e-mails, permitindo injeção de dependência.  
- `EmailService`: Implementa o envio via SendGrid, usando configurações do `appsettings.json`.  
- O conteúdo é enviado como HTML para suportar links clicáveis.

Registre o serviço no `Program.cs`:

```csharp
builder.Services.AddSingleton<IEmailService, EmailService>(); // Registra EmailService como singleton
```

---

## 6. 🎮 AuthController (Apenas E-mail)

Adicione métodos para confirmação de e-mail e recuperação de senha ao `AuthController`.

```csharp
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Web;

// Controlador para funcionalidades de e-mail
[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager; // Gerencia usuários
    private readonly IEmailService _emailService; // Serviço de e-mail

    public AuthController(
        UserManager<ApplicationUser> userManager,
        IEmailService emailService)
    {
        _userManager = userManager;
        _emailService = emailService;
    }

    // Registra usuário e envia e-mail de confirmação
    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register(RegisterDTO dto)
    {
        // Cria usuário
        var user = new ApplicationUser
        {
            UserName = dto.UserName,
            Email = dto.Email,
            NomeCompleto = dto.NomeCompleto
        };

        // Salva usuário
        var result = await _userManager.CreateAsync(user, dto.Password);
        if (!result.Succeeded)
            return BadRequest(new { Errors = result.Errors });

        // Gera token de confirmação
        var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
        // Cria link de confirmação
        var confirmLink = Url.Action(
            "ConfirmEmail",
            "Auth",
            new { userId = user.Id, token = HttpUtility.UrlEncode(token) },
            Request.Scheme
        );

        // Monta e-mail HTML
        var htmlContent = $@"<h2>Bem-vindo, {user.NomeCompleto}!</h2>
            <p>Por favor, confirme seu e-mail clicando no link abaixo:</p>
            <a href=""{confirmLink}"">Confirmar E-mail</a>";

        // Envia e-mail
        await _emailService.SendEmailAsync(
            user.Email,
            "Confirme sua Conta",
            htmlContent
        );

        return Ok(new { Message = "Registro bem-sucedido. Verifique seu e-mail." });
    }

    // Confirma o e-mail do usuário
    [HttpGet("confirm-email")]
    [AllowAnonymous]
    public async Task<IActionResult> ConfirmEmail(string userId, string token)
    {
        // Valida parâmetros
        if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(token))
            return BadRequest(new { Message = "Link de confirmação inválido." });

        // Busca usuário
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return NotFound(new { Message = "Usuário não encontrado." });

        // Confirma e-mail
        var result = await _userManager.ConfirmEmailAsync(user, token);
        if (result.Succeeded)
            return Ok(new { Message = "E-mail confirmado com sucesso!" });
        return BadRequest(new { Errors = result.Errors });
    }

    // Inicia recuperação de senha
    [HttpPost("forgot-password")]
    [AllowAnonymous]
    public async Task<IActionResult> ForgotPassword([FromBody] string email)
    {
        // Busca usuário
        var user = await _userManager.FindByEmailAsync(email);
        // Resposta genérica para segurança
        if (user == null || !await _userManager.IsEmailConfirmedAsync(user))
            return Ok(new { Message = "Se o e-mail estiver cadastrado, um link será enviado." });

        // Gera token de redefinição
        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        // Cria link
        var resetLink = Url.Action(
            "ResetPassword",
            "Auth",
            new { userId = user.Id, token = HttpUtility.UrlEncode(token) },
            Request.Scheme
        );

        // Monta e-mail HTML
        var htmlContent = $@"<h2>Redefinição de Senha</h2>
            <p>Para redefinir sua senha, clique no link abaixo:</p>
            <a href=""{resetLink}"">Redefinir Senha</a>";

        // Envia e-mail
        await _emailService.SendEmailAsync(
            user.Email,
            "Redefinir sua Senha",
            htmlContent
        );

        return Ok(new { Message = "Link de recuperação enviado." });
    }

    // Redefine a senha
    [HttpPost("reset-password")]
    [AllowAnonymous]
    public async Task<IActionResult> ResetPassword(ResetPasswordDTO dto)
    {
        // Busca usuário
        var user = await _userManager.FindByEmailAsync(dto.Email);
        if (user == null)
            return BadRequest(new { Message = "Usuário não encontrado." });

        // Redefine senha
        var result = await _userManager.ResetPasswordAsync(user, dto.Token, dto.NovaSenha);
        if (result.Succeeded)
            return Ok(new { Message = "Senha redefinida com sucesso!" });
        return BadRequest(new { Errors = result.Errors });
    }
}
```

**Explicação**:  
- `Register`: Cria usuário, gera token de confirmação, e envia e-mail com link.  
- `ConfirmEmail`: Valida o token e confirma o e-mail.  
- `ForgotPassword`: Gera token de redefinição e envia e-mail com link.  
- `ResetPassword`: Redefine a senha com o token fornecido.  
- Usa `HttpUtility.UrlEncode` para garantir que tokens sejam seguros em URLs.

---

## 7. 📌 Boas Práticas e Segurança

- **Validação**: Use Data Annotations (`[Required]`, `[EmailAddress]`) nos DTOs.  
- **Segurança de Chaves**: Armazene a chave API do SendGrid em variáveis de ambiente.  
- **HTML Seguro**: Use HTML simples nos e-mails para evitar XSS; sanitize entradas se necessário.  
- **Limite de Envio**: Implemente rate limiting para evitar abuso (ex.: limitar requisições de `forgot-password`).  
- **Fallbacks**: Trate erros de envio (ex.: falhas na API do SendGrid) com logs.  
- **Testes**: Crie testes para verificar o envio de e-mails (ex.: mock do `IEmailService`).  
- **Alternativas**: Considere SMTP (ex.: Gmail, Outlook) para cenários sem SendGrid:
  ```csharp
  var smtpClient = new SmtpClient("smtp.gmail.com")
  {
      Port = 587,
      Credentials = new NetworkCredential("seu-email@gmail.com", "sua-senha"),
      EnableSsl = true
  };
  await smtpClient.SendMailAsync(new MailMessage(
      "seu-email@gmail.com",
      toEmail,
      subject,
      htmlContent
  ) { IsBodyHtml = true });
  ```
- **Logging**: Registre tentativas de envio para auditoria.

---

## 8. 📋 Tabela de Endpoints

| Método | Endpoint                    | Descrição                              | Autenticação |
|--------|-----------------------------|----------------------------------------|--------------|
| POST   | `/api/auth/register`        | Registra usuário e envia e-mail        | Anônimo      |
| GET    | `/api/auth/confirm-email`   | Confirma e-mail com token              | Anônimo      |
| POST   | `/api/auth/forgot-password` | Envia e-mail de recuperação            | Anônimo      |
| POST   | `/api/auth/reset-password`  | Redefine senha com token               | Anônimo      |

```

