
# Complementos à Implementação JWT e ASP.NET Core Identity

## Implementação de Claims Personalizadas

Vamos adicionar suporte para claims personalizadas que podem enriquecer a autorização:

```csharp
// Services/ClaimService.cs
using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using YourNamespace.Models.Identity;

namespace YourNamespace.Services
{
    public interface IClaimService
    {
        Task<IList<Claim>> GetUserClaimsAsync(string userId);
        Task<IdentityResult> AddClaimToUserAsync(string userId, string claimType, string claimValue);
        Task<IdentityResult> RemoveClaimFromUserAsync(string userId, string claimType, string claimValue);
    }

    public class ClaimService : IClaimService
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public ClaimService(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<IList<Claim>> GetUserClaimsAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return new List<Claim>();

            return await _userManager.GetClaimsAsync(user);
        }

        public async Task<IdentityResult> AddClaimToUserAsync(string userId, string claimType, string claimValue)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return IdentityResult.Failed(new IdentityError { Description = "Usuário não encontrado" });

            return await _userManager.AddClaimAsync(user, new Claim(claimType, claimValue));
        }

        public async Task<IdentityResult> RemoveClaimFromUserAsync(string userId, string claimType, string claimValue)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return IdentityResult.Failed(new IdentityError { Description = "Usuário não encontrado" });

            return await _userManager.RemoveClaimAsync(user, new Claim(claimType, claimValue));
        }
    }
}
```

## Políticas de Autorização Avançadas

Criando políticas de autorização mais avançadas:

```csharp
// Auth/AuthorizationPolicies.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;

namespace YourNamespace.Auth
{
    public static class AuthorizationPolicies
    {
        public static void ConfigurePolicies(IServiceCollection services)
        {
            services.AddAuthorization(options =>
            {
                // Políticas baseadas em claims
                options.AddPolicy("AdminOrManager", policy =>
                    policy.RequireAssertion(context =>
                        context.User.IsInRole("Admin") || context.User.IsInRole("Manager")));

                options.AddPolicy("CanAccessReports", policy =>
                    policy.RequireClaim("Permission", "AccessReports"));

                // Política com requisito personalizado
                options.AddPolicy("MinimumTenure", policy =>
                    policy.AddRequirements(new MinimumTenureRequirement(2))); // 2 anos
            });

            // Registrar handler para o requisito personalizado
            services.AddSingleton<IAuthorizationHandler, MinimumTenureHandler>();
        }
    }

    // Requisito personalizado
    public class MinimumTenureRequirement : IAuthorizationRequirement
    {
        public int YearsRequired { get; }

        public MinimumTenureRequirement(int yearsRequired)
        {
            YearsRequired = yearsRequired;
        }
    }

    // Handler para o requisito personalizado
    public class MinimumTenureHandler : AuthorizationHandler<MinimumTenureRequirement>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, MinimumTenureRequirement requirement)
        {
            if (!context.User.HasClaim(c => c.Type == "JoinYear"))
                return Task.CompletedTask;

            var joinYearClaim = context.User.FindFirst(c => c.Type == "JoinYear");
            if (int.TryParse(joinYearClaim.Value, out int joinYear))
            {
                var yearsAtCompany = System.DateTime.Now.Year - joinYear;
                if (yearsAtCompany >= requirement.YearsRequired)
                {
                    context.Succeed(requirement);
                }
            }

            return Task.CompletedTask;
        }
    }
}
```

Chame a configuração no `Program.cs`:

```csharp
// Program.cs
AuthorizationPolicies.ConfigurePolicies(builder.Services);
```

## Monitor de Segurança e Detecção de Anomalias

Crie um serviço para monitorar tentativas de login suspeitas:

```csharp
// Services/SecurityMonitor.cs
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using YourNamespace.Models.Identity;

namespace YourNamespace.Services
{
    public interface ISecurityMonitor
    {
        Task RecordLoginAttemptAsync(string username, string ipAddress, bool success);
        Task<bool> CheckForSuspiciousActivityAsync(string username, string ipAddress);
    }

    public class SecurityMonitor : ISecurityMonitor
    {
        private readonly ILogger<SecurityMonitor> _logger;
        private readonly UserManager<ApplicationUser> _userManager;
        private static readonly Dictionary<string, List<(DateTime timestamp, string ipAddress, bool success)>> _loginAttempts = new();
        
        // Limites de segurança
        private const int MaxFailedAttemptsAllowed = 5;
        private const int SuspiciousTimeWindowMinutes = 10;
        
        public SecurityMonitor(
            ILogger<SecurityMonitor> logger,
            UserManager<ApplicationUser> userManager)
        {
            _logger = logger;
            _userManager = userManager;
        }

        public Task RecordLoginAttemptAsync(string username, string ipAddress, bool success)
        {
            if (string.IsNullOrEmpty(username))
                return Task.CompletedTask;

            if (!_loginAttempts.ContainsKey(username))
            {
                _loginAttempts[username] = new List<(DateTime, string, bool)>();
            }

            _loginAttempts[username].Add((DateTime.UtcNow, ipAddress, success));
            
            // Limpar histórico antigo (mais de 24 horas)
            _loginAttempts[username] = _loginAttempts[username]
                .Where(a => a.timestamp > DateTime.UtcNow.AddHours(-24))
                .ToList();

            return Task.CompletedTask;
        }

        public async Task<bool> CheckForSuspiciousActivityAsync(string username, string ipAddress)
        {
            if (string.IsNullOrEmpty(username) || !_loginAttempts.ContainsKey(username))
                return false;

            // Verificar tentativas falhas recentes do mesmo usuário
            var recentFailedAttempts = _loginAttempts[username]
                .Where(a => !a.success && a.timestamp > DateTime.UtcNow.AddMinutes(-SuspiciousTimeWindowMinutes))
                .Count();

            if (recentFailedAttempts >= MaxFailedAttemptsAllowed)
            {
                _logger.LogWarning(
                    "Atividade suspeita detectada para usuário {Username}: {FailedAttempts} falhas em {Minutes} minutos",
                    username, recentFailedAttempts, SuspiciousTimeWindowMinutes);
                
                // Ação opcional: bloquear temporariamente o usuário
                var user = await _userManager.FindByNameAsync(username);
                if (user != null)
                {
                    await _userManager.SetLockoutEndDateAsync(user, DateTime.UtcNow.AddMinutes(15));
                    _logger.LogWarning("Usuário {Username} bloqueado temporariamente por 15 minutos", username);
                }
                
                return true;
            }

            // Verificar logins de múltiplos IPs diferentes
            var distinctIps = _loginAttempts[username]
                .Where(a => a.timestamp > DateTime.UtcNow.AddMinutes(-SuspiciousTimeWindowMinutes))
                .Select(a => a.ipAddress)
                .Distinct()
                .Count();

            if (distinctIps >= 3) // Mais de 3 IPs diferentes em pouco tempo é suspeito
            {
                _logger.LogWarning(
                    "Atividade suspeita detectada para usuário {Username}: {DistinctIPs} IPs distintos em {Minutes} minutos",
                    username, distinctIps, SuspiciousTimeWindowMinutes);
                return true;
            }

            return false;
        }
    }
}
```

## Controller para Recuperação de Senha

Adicione funcionalidade de recuperação de senha:

```csharp
// Controllers/PasswordController.cs
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Text;
using System.Threading.Tasks;
using YourNamespace.Models.Identity;

namespace YourNamespace.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PasswordController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailService _emailService; // Você precisará implementar este serviço

        public PasswordController(
            UserManager<ApplicationUser> userManager,
            IEmailService emailService)
        {
            _userManager = userManager;
            _emailService = emailService;
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
                return Ok(); // Não revelar se o email existe ou não por segurança

            // Gerar token de redefinição de senha
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            
            // URL para redefinição (frontend precisa processar esta URL)
            var resetLink = $"{model.ClientBaseUrl}/reset-password?email={Uri.EscapeDataString(model.Email)}&token={Uri.EscapeDataString(token)}";
            
            // Enviar email
            await _emailService.SendEmailAsync(
                model.Email,
                "Redefinição de Senha",
                $"Para redefinir sua senha, clique no link: <a href='{resetLink}'>Redefinir senha</a>");

            return Ok();
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
                return BadRequest("Usuário não encontrado");

            var result = await _userManager.ResetPasswordAsync(user, model.Token, model.NewPassword);
            if (!result.Succeeded)
                return BadRequest(result.Errors);

            return Ok();
        }

        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await _userManager.FindByNameAsync(User.Identity.Name);
            if (user == null)
                return BadRequest("Usuário não encontrado");

            var result = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
            if (!result.Succeeded)
                return BadRequest(result.Errors);

            return Ok();
        }
    }

    public class ForgotPasswordDto
    {
        public string Email { get; set; }
        public string ClientBaseUrl { get; set; } // URL base do cliente para redirecionamento
    }

    public class ResetPasswordDto
    {
        public string Email { get; set; }
        public string Token { get; set; }
        public string NewPassword { get; set; }
    }

    public class ChangePasswordDto
    {
        public string CurrentPassword { get; set; }
        public string NewPassword { get; set; }
    }
}
```

## Integração de Autenticação Externa

Configure autenticação com provedores externos (Google, Facebook, etc.):

```csharp
// Program.cs (adicionando aos serviços)
builder.Services.AddAuthentication()
    .AddGoogle(options =>
    {
        options.ClientId = builder.Configuration["Authentication:Google:ClientId"];
        options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"];
        options.CallbackPath = "/api/auth/google-callback";
    })
    .AddFacebook(options =>
    {
        options.AppId = builder.Configuration["Authentication:Facebook:AppId"];
        options.AppSecret = builder.Configuration["Authentication:Facebook:AppSecret"];
        options.CallbackPath = "/api/auth/facebook-callback";
    });
```

E um controller para gerenciar logins externos:

```csharp
// Controllers/ExternalAuthController.cs
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;
using YourNamespace.Models.Auth;
using YourNamespace.Models.Identity;
using YourNamespace.Services;

namespace YourNamespace.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ExternalAuthController : ControllerBase
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ITokenService _tokenService;

        public ExternalAuthController(
            SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager,
            ITokenService tokenService)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _tokenService = tokenService;
        }

        [HttpGet("login/{provider}")]
        public IActionResult ExternalLogin(string provider, string returnUrl = null)
        {
            var redirectUrl = Url.Action(nameof(ExternalLoginCallback), "ExternalAuth", new { returnUrl });
            var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
            return Challenge(properties, provider);
        }

        [HttpGet("callback")]
        public async Task<IActionResult> ExternalLoginCallback(string returnUrl = null)
        {
            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
                return BadRequest("Erro ao obter informações de login externo.");

            // Tentativa de login
            var result = await _signInManager.ExternalLoginSignInAsync(
                info.LoginProvider, info.ProviderKey, isPersistent: false, bypassTwoFactor: true);

            if (result.Succeeded)
            {
                // Usuário já existe, gerar tokens e redirecionar
                var user = await _userManager.FindByLoginAsync(info.LoginProvider, info.ProviderKey);
                var accessToken = await _tokenService.GenerateAccessTokenAsync(user);
                var refreshToken = await _tokenService.GenerateRefreshTokenAsync();

                // Salvar refresh token
                user.RefreshToken = refreshToken;
                user.RefreshTokenExpiryTime = System.DateTime.UtcNow.AddDays(7);
                await _userManager.UpdateAsync(user);

                // Redirecionar para URL do cliente com tokens
                var tokenInfo = new
                {
                    accessToken,
                    refreshToken
                };
                
                // Na prática, você redirecionaria para o frontend com os tokens
                return Ok(tokenInfo);
            }
            else
            {
                // Usuário não existe, criar novo usuário
                var email = info.Principal.FindFirstValue(ClaimTypes.Email);
                var user = new ApplicationUser
                {
                    UserName = email,
                    Email = email,
                    FullName = info.Principal.FindFirstValue(ClaimTypes.Name) ?? ""
                };

                var createResult = await _userManager.CreateAsync(user);
                if (createResult.Succeeded)
                {
                    // Adicionar login externo ao usuário
                    var addLoginResult = await _userManager.AddLoginAsync(user, info);
                    if (addLoginResult.Succeeded)
                    {
                        // Gerar tokens
                        var accessToken = await _tokenService.GenerateAccessTokenAsync(user);
                        var refreshToken = await _tokenService.GenerateRefreshTokenAsync();

                        // Salvar refresh token
                        user.RefreshToken = refreshToken;
                        user.RefreshTokenExpiryTime = System.DateTime.UtcNow.AddDays(7);
                        await _userManager.UpdateAsync(user);

                        // Redirecionar para URL do cliente com tokens
                        var tokenInfo = new
                        {
                            accessToken,
                            refreshToken
                        };
                        
                        return Ok(tokenInfo);
                    }
                }
                
                return BadRequest("Erro ao criar conta com login externo.");
            }
        }
    }
}
```

