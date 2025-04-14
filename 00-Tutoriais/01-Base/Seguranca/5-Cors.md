🌐 Configurando CORS no ASP.NET Core 8
Este tutorial explica como configurar CORS (Cross-Origin Resource Sharing) em um projeto ASP.NET Core 8, integrado ao sistema de autenticação e autorização descrito anteriormente (usando Identity, JWT, e-mails, login com Google e 2FA). O CORS permite que aplicações frontend hospedadas em domínios diferentes (como http://localhost:3000) acessem a API backend (como https://localhost:5001) de forma segura.

📘 Índice

O que é CORS e Por que Usar?
Pré-requisitos
Configuração Básica do CORS
Configuração Avançada com Políticas
Integração com o Controller de Usuários
Testando o CORS
Boas Práticas e Considerações


1. ❓ O que é CORS e Por que Usar?
CORS é um mecanismo de segurança dos navegadores que controla quais origens (domínios, protocolos e portas) podem acessar recursos de uma API. Por padrão, navegadores bloqueiam requisições de origens diferentes da API para evitar ataques como CSRF (Cross-Site Request Forgery).
Por que usar CORS?

Permite que um frontend (ex.: SPA em React ou Angular) em http://localhost:3000 acesse uma API em https://localhost:5001.
Garante segurança ao limitar quais origens, métodos HTTP e cabeçalhos são permitidos.
Essencial para aplicações modernas com frontend e backend separados.

Quando configurar CORS?

Sempre que sua API for acessada por um frontend em um domínio diferente.
Em cenários com autenticação (como JWT ou login com Google), para garantir que tokens e cookies sejam manipulados corretamente.


2. 📦 Pré-requisitos

Projeto Existente: Um projeto ASP.NET Core 8 com autenticação configurada (Identity, JWT, e-mails, Google, 2FA), conforme os tutoriais anteriores.
Pacotes: Não é necessário adicionar pacotes extras, pois o suporte a CORS está incluído no pacote Microsoft.AspNetCore.App.
Frontend: Um frontend (como uma SPA) para testar as requisições cross-origin.


3. ⚙️ Configuração Básica do CORS
A configuração mais simples permite todas as origens, métodos e cabeçalhos, mas não é recomendada para produção devido a riscos de segurança.
Configuração no Program.cs
Adicione o CORS ao pipeline no Program.cs:
// Adicionar serviço de CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});

// Configurar pipeline
app.UseHttpsRedirection();

// Adicionar CORS antes de autenticação e autorização
app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

Explicação:

AddCors: Registra o serviço de CORS com uma política chamada AllowAll.
AllowAnyOrigin: Permite requisições de qualquer origem (ex.: http://localhost:3000, https://meuapp.com).
AllowAnyMethod: Permite todos os métodos HTTP (GET, POST, PUT, etc.).
AllowAnyHeader: Permite todos os cabeçalhos (ex.: Authorization, Content-Type).
UseCors: Aplica a política ao pipeline, antes da autenticação para garantir que as requisições sejam validadas.


Aviso: AllowAnyOrigin é útil para desenvolvimento, mas em produção você deve restringir as origens permitidas.


4. 🔐 Configuração Avançada com Políticas
Para maior segurança, configure políticas específicas que restringem:

Origens permitidas: Apenas domínios confiáveis.
Métodos HTTP: Apenas GET, POST, etc., conforme necessário.
Cabeçalhos: Apenas os necessários, como Authorization.
Credenciais: Suporte a cookies ou tokens JWT.

Exemplo de Política Restrita
Atualize o Program.cs com uma política mais segura:
builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontendPolicy", builder =>
    {
        builder.WithOrigins(
                "http://localhost:3000", // Frontend em desenvolvimento
                "https://meuapp.com"     // Frontend em produção
            )
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials(); // Suporta cookies ou tokens autenticados
    });
});

// Configurar pipeline
app.UseHttpsRedirection();

app.UseCors("FrontendPolicy");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

Explicação:

WithOrigins: Especifica exatamente quais origens são permitidas.
AllowCredentials: Necessário para enviar cookies ou cabeçalhos Authorization com tokens JWT.
AllowAnyMethod e AllowAnyHeader: Mantidos para flexibilidade, mas podem ser restritos (ex.: .WithMethods("GET", "POST")).

Aplicando CORS por Endpoint
Você pode aplicar políticas específicas a controllers ou ações usando o atributo [EnableCors]:
[ApiController]
[Route("api/usuarios")]
[EnableCors("FrontendPolicy")]
public class UsuariosController : ControllerBase
{
    // Métodos do controller
}

Ou aplicar a uma ação específica:
[HttpPost("login")]
[EnableCors("FrontendPolicy")]
public async Task<IActionResult> Login(LoginDTO dto)
{
    // Lógica de login
}


5. 🔗 Integração com o Controller de Usuários
O controller de usuários (dos tutoriais anteriores) já funciona com CORS, mas é importante garantir que ele lida corretamente com requisições cross-origin, especialmente para endpoints como login, login com Google, e 2FA, que podem enviar cabeçalhos Authorization ou cookies.
Controller Exemplo
Abaixo está um trecho do controller atualizado, destacando a compatibilidade com CORS:
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
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
    [EnableCors("FrontendPolicy")]
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

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login(LoginDTO dto)
        {
            var user = await _userManager.FindByNameAsync(dto.UserName);
            if (user == null || !await _userManager.CheckPasswordAsync(user, dto.Password))
                return Unauthorized(new { Message = "Credenciais inválidas" });

            if (await _userManager.GetTwoFactorEnabledAsync(user))
                return Ok(new { Message = "2FA necessário", RequiresTwoFactor = true });

            var token = await GenerateJwtToken(user);
            return Ok(token);
        }

        [HttpGet("login-google")]
        [AllowAnonymous]
        public IActionResult LoginGoogle()
        {
            var redirectUrl = Url.Action("GoogleCallback", "Usuarios", null, Request.Scheme);
            var properties = _signInManager.ConfigureExternalAuthenticationProperties("Google", redirectUrl);
            return new ChallengeResult("Google", properties);
        }

        // Outros métodos (2FA, registro, etc.) permanecem como nos tutoriais anteriores

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

Notas:

O atributo [EnableCors("FrontendPolicy")] no controller garante que todos os endpoints respeitem a política configurada.
Para endpoints como login-google, o CORS é crítico, pois o frontend pode iniciar o fluxo de autenticação de um domínio diferente.
O cabeçalho Authorization (usado com JWT) é permitido automaticamente pela política AllowAnyHeader.


6. 🧪 Testando o CORS
Para verificar se o CORS está funcionando:

Configure um Frontend:

Crie uma SPA (ex.: em React) rodando em http://localhost:3000.
Faça uma requisição para a API (ex.: fetch('https://localhost:5001/api/usuarios/login', { method: 'POST', ... })).


Teste com Ferramentas:

Use o Postman ou cURL com a opção --origin http://localhost:3000 para simular requisições cross-origin.
Exemplo com cURL:curl -X POST https://localhost:5001/api/usuarios/login \
-H "Origin: http://localhost:3000" \
-H "Content-Type: application/json" \
-d '{"userName":"teste","password":"Senha123!"}'




Inspecione Respostas:

Verifique os cabeçalhos da resposta no console do navegador (aba Network):
Access-Control-Allow-Origin: Deve conter a origem permitida (ex.: http://localhost:3000).
Access-Control-Allow-Credentials: Deve ser true se AllowCredentials estiver configurado.




Erros Comuns:

"No 'Access-Control-Allow-Origin' header is present": A política CORS não está configurada ou a origem não está permitida.
"CORS preflight request failed": Verifique se métodos como OPTIONS estão permitidos e se a política inclui os cabeçalhos necessários.




7. 📌 Boas Práticas e Considerações

Restringir Origens: Em produção, sempre use WithOrigins para permitir apenas domínios confiáveis. Evite AllowAnyOrigin com AllowCredentials, pois isso pode expor a API a ataques.
Métodos Específicos: Restrinja métodos HTTP (ex.: .WithMethods("GET", "POST")) para endpoints que não precisam de outros métodos.
Cabeçalhos Necessários: Liste explicitamente os cabeçalhos permitidos (ex.: .WithHeaders("Authorization", "Content-Type")) para maior segurança.
Credenciais: Use AllowCredentials apenas quando necessário (ex.: para cookies ou tokens JWT) e combine com origens específicas.
Preflight Requests: O ASP.NET Core lida automaticamente com requisições OPTIONS (preflight), mas certifique-se de que a política cobre todos os métodos e cabeçalhos usados.
Segurança com JWT: Verifique se o token JWT é enviado no cabeçalho Authorization e validado corretamente, mesmo com CORS habilitado.
Testes: Teste o CORS em diferentes navegadores e com ferramentas como Postman para garantir compatibilidade.
Logs: Monitore erros de CORS (use Serilog ou Application Insights) para diagnosticar problemas rapidamente.
Integração com Autenticação Externa: Para login com Google, o CORS deve permitir redirecionamentos e callbacks do frontend para a API.


Este tutorial cobre a configuração de CORS no ASP.NET Core 8, integrado ao sistema de autenticação existente. Com isso, sua API estará pronta para ser acessada por frontends em domínios diferentes, mantendo segurança e funcionalidade.
