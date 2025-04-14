

# 📧 Envio de E-mails para Autenticação com ASP.NET Core 8

Este tutorial complementa o sistema de autenticação e autorização com **ASP.NET Core 8**, focando na implementação do envio de e-mails para **confirmação de conta** e **recuperação de senha**. Usaremos o protocolo **SMTP** (com um exemplo baseado no Gmail) para demonstração, mas também mencionaremos alternativas como **SendGrid** para cenários de produção.

---

## 📘 Índice

1. [Por que Enviar E-mails?](#1-por-que-enviar-e-mails)
2. [Pacotes Necessários](#2-pacotes-necessários)
3. [Configuração do Serviço de E-mail](#3-configuração-do-serviço-de-e-mail)
4. [Modelo de E-mail e Interface de Serviço](#4-modelo-de-e-mail-e-interface-de-serviço)
5. [Implementação do Serviço de E-mail](#5-implementação-do-serviço-de-e-mail)
6. [Integração com o Controller de Usuários](#6-integração-com-o-controller-de-usuários)
7. [Alternativas ao SMTP](#7-alternativas-ao-smtp)
8. [Boas Práticas e Considerações](#8-boas-práticas-e-considerações)

---

## 1. ❓ Por que Enviar E-mails?

O envio de e-mails é essencial em sistemas de autenticação para:

- **Confirmação de Conta**: Garante que o e-mail fornecido pelo usuário é válido antes de ativar a conta.
- **Recuperação de Senha**: Permite que os usuários redefinam suas senhas de forma segura enviando um token de redefinição.
- **Notificações**: Informa os usuários sobre atividades importantes, como tentativas de login ou alterações de conta.

---

## 2. 📦 Pacotes Necessários

Para enviar e-mails usando SMTP, usaremos a biblioteca padrão **System.Net.Mail** do .NET. Para cenários mais avançados, podemos adicionar pacotes como:

```bash
dotnet add package MailKit
```

**MailKit** é uma biblioteca robusta para envio de e-mails que suporta SMTP, IMAP e POP3. Neste tutorial, usaremos **MailKit** por ser mais flexível e amplamente adotado.

---

## 3. ⚙️ Configuração do Serviço de E-mail

### Configurar no `appsettings.json`

Adicione as configurações do serviço de e-mail ao arquivo `appsettings.json`. Para o exemplo com Gmail:

```json
{
  "EmailSettings": {
    "SmtpServer": "smtp.gmail.com",
    "SmtpPort": 587,
    "SenderName": "Sua Aplicação",
    "SenderEmail": "seu-email@gmail.com",
    "Username": "seu-email@gmail.com",
    "Password": "sua-senha-de-app"
  }
}
```

> **Nota sobre Gmail**: Para usar o Gmail, você precisa gerar uma **senha de aplicativo** nas configurações de segurança da sua conta Google, pois a autenticação de dois fatores exige isso. **Nunca** armazene senhas diretamente no `appsettings.json` em produção; use **Azure Key Vault**, **AWS Secrets Manager** ou variáveis de ambiente.

### Registrar o Serviço no `Program.cs`

Configure o serviço de e-mail como um singleton no `Program.cs`:

```csharp
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
builder.Services.AddSingleton<IEmailService, EmailService>();
```

---

## 4. 📝 Modelo de E-mail e Interface de Serviço

### Modelo `EmailSettings`

Crie uma classe para mapear as configurações do `appsettings.json`:

```csharp
public class EmailSettings
{
    public string SmtpServer { get; set; } = null!;
    public int SmtpPort { get; set; }
    public string SenderName { get; set; } = null!;
    public string SenderEmail { get; set; } = null!;
    public string Username { get; set; } = null!;
    public string Password { get; set; } = null!;
}
```

### Interface `IEmailService`

Defina uma interface para o serviço de e-mail:

```csharp
public interface IEmailService
{
    Task SendEmailAsync(string toEmail, string subject, string body, bool isHtml = false);
}
```

---

## 5. ✉️ Implementação do Serviço de E-mail

Crie a implementação do serviço de e-mail usando **MailKit**:

```csharp
using MailKit.Net.Smtp;
using MimeKit;
using Microsoft.Extensions.Options;

public class EmailService : IEmailService
{
    private readonly EmailSettings _settings;

    public EmailService(IOptions<EmailSettings> settings)
    {
        _settings = settings.Value;
    }

    public async Task SendEmailAsync(string toEmail, string subject, string body, bool isHtml = false)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_settings.SenderName, _settings.SenderEmail));
        message.To.Add(new MailboxAddress("", toEmail));
        message.Subject = subject;

        var bodyBuilder = new BodyBuilder();
        if (isHtml)
            bodyBuilder.HtmlBody = body;
        else
            bodyBuilder.TextBody = body;

        message.Body = bodyBuilder.ToMessageBody();

        using var client = new SmtpClient();
        await client.ConnectAsync(_settings.SmtpServer, _settings.SmtpPort, MailKit.Security.SecureSocketOptions.StartTls);
        await client.AuthenticateAsync(_settings.Username, _settings.Password);
        await client.SendAsync(message);
        await client.DisconnectAsync(true);
    }
}
```

---

## 6. 🔗 Integração com o Controller de Usuários

Atualize o controller de usuários (baseado no resumo anterior) para incluir o envio de e-mails nos endpoints de **registro**, **confirmação de e-mail** e **recuperação de senha**.

### Controller Atualizado

```csharp
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Web;

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
            var confirmationLink = Url.Action("ConfirmarEmail", "Usuarios", 
                new { userId = user.Id, token = HttpUtility.UrlEncode(token) }, 
                Request.Scheme);

            var emailBody = $@"<h2>Bem-vindo, {dto.NomeCompleto}!</h2>
                             <p>Por favor, confirme seu e-mail clicando no link abaixo:</p>
                             <a href='{confirmationLink}'>Confirmar E-mail</a>";

            await _emailService.SendEmailAsync(dto.Email, "Confirme seu E-mail", emailBody, true);

            return Ok(new { Message = "Registro bem-sucedido. Verifique seu e-mail." });
        }

        [HttpGet("confirmar-email")]
        [AllowAnonymous]
        public async Task<IActionResult> ConfirmarEmail(string userId, string token)
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(token))
                return BadRequest(new { Message = "Parâmetros inválidos" });

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return NotFound(new { Message = "Usuário não encontrado" });

            var result = await _userManager.ConfirmEmailAsync(user, token);
            if (!result.Succeeded)
                return BadRequest(result.Errors);

            return Ok(new { Message = "E-mail confirmado com sucesso!" });
        }

        [HttpPost("esqueci-senha")]
        [AllowAnonymous]
        public async Task<IActionResult> EsqueciSenha([FromBody] string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                return BadRequest(new { Message = "Usuário não encontrado" });

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var resetLink = Url.Action("ResetarSenha", "Usuarios", 
                new { email = user.Email, token = HttpUtility.UrlEncode(token) }, 
                Request.Scheme);

            var emailBody = $@"<h2>Redefinição de Senha</h2>
                             <p>Para redefinir sua senha, clique no link abaixo:</p>
                             <a href='{resetLink}'>Redefinir Senha</a>";

            await _emailService.SendEmailAsync(email, "Redefinir Senha", emailBody, true);

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

        // Outros métodos (login, refresh-token, etc.) permanecem como no resumo anterior
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

### DTOs Necessários

Os DTOs já foram definidos no resumo anterior, mas para referência:

```csharp
public class RegisterDTO
{
    public string UserName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string Password { get; set; } = null!;
    public string NomeCompleto { get; set; } = null!;
}

public class ResetPasswordDTO
{
    public string Email { get; set; } = null!;
    public string Token { get; set; } = null!;
    public string NovaSenha { get; set; } = null!;
}

public class RespuestaAutenticacionDTO
{
    public string Token { get; set; } = null!;
    public string RefreshToken { get; set; } = null!;
    public DateTime Expiracion { get; set; }
}
```

---

## 7. 🌐 Alternativas ao SMTP

Embora o SMTP seja simples para testes, em produção é recomendável usar serviços especializados para maior confiabilidade e escalabilidade:

- **SendGrid**:
  - Pacote: `dotnet add package SendGrid`
  - Configuração: Substitua o `EmailService` para usar a API do SendGrid.
  - Vantagem: Alta taxa de entrega e relatórios detalhados.

- **Amazon SES (Simple Email Service)**:
  - Pacote: `dotnet add package AWSSDK.SimpleEmail`
  - Vantagem: Integração nativa com AWS e baixo custo.

- **Mailgun**:
  - Pacote: `dotnet add package RestSharp` (para chamadas HTTP à API do Mailgun).
  - Vantagem: Fácil configuração e suporte a templates.

Para usar o **SendGrid**, por exemplo, você pode implementar assim:

```csharp
using SendGrid;
using SendGrid.Helpers.Mail;

public class EmailService : IEmailService
{
    private readonly string _apiKey;

    public EmailService(IConfiguration configuration)
    {
        _apiKey = configuration["SendGrid:ApiKey"];
    }

    public async Task SendEmailAsync(string toEmail, string subject, string body, bool isHtml = false)
    {
        var client = new SendGridClient(_apiKey);
        var from = new EmailAddress("seu-email@exemplo.com", "Sua Aplicação");
        var to = new EmailAddress(toEmail);
        var msg = MailHelper.CreateSingleEmail(from, to, subject, isHtml ? null : body, isHtml ? body : null);
        await client.SendEmailAsync(msg);
    }
}
```

Adicione a chave API no `appsettings.json`:

```json
{
  "SendGrid": {
    "ApiKey": "sua-chave-api-sendgrid"
  }
}
```

---

## 8. 📌 Boas Práticas e Considerações

- **Segurança das Credenciais**: Armazene senhas e chaves API em gerenciadores de segredos (como Azure Key Vault ou variáveis de ambiente).
- **Templates de E-mail**: Use templates HTML para e-mails profissionais. Ferramentas como **MJML** podem ajudar a criar layouts responsivos.
- **Limite de Envio**: Implemente filas (como **Hangfire** ou **Azure Queue Storage**) para evitar sobrecarga no envio de e-mails em massa.
- **Logs**: Registre falhas de envio para monitoramento (use **Serilog** ou **Application Insights**).
- **Testes Locais**: Use ferramentas como **MailHog** ou **Ethereal** para testar e-mails sem enviá-los realmente.
- **Validação**: Sempre valide o e-mail antes de enviar (por exemplo, verifique se o usuário existe).
- **Rate Limiting**: Proteja endpoints como `esqueci-senha` contra abusos com limites de requisições.

---

Este tutorial cobre a implementação completa do envio de e-mails para autenticação, integrado ao sistema de **ASP.NET Core 8** descrito anteriormente. Com isso, você tem um sistema robusto que inclui **confirmação de e-mail**, **recuperação de senha** e **boas práticas de segurança**.

