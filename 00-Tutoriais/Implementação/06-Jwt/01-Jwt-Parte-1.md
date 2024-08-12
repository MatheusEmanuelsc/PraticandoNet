
---

### **Implementa��o de JWT, Identity e Refresh Tokens no ASP.NET Core**

Este guia fornece um passo a passo para configurar autentica��o JWT, Identity e Refresh Tokens em uma aplica��o ASP.NET Core. O tutorial est� dividido em v�rias partes, cada uma abordando uma etapa espec�fica da implementa��o.

## **�ndice**

1. [Parte 1 - Configura��o Inicial de JWT](#parte-1---configura��o-inicial-de-jwt)
   1. [Etapa 1 - Instalar Pacote JWT Bearer](#etapa-1---instalar-pacote-jwt-bearer)
   2. [Etapa 2 - Configura��o do Program.cs](#etapa-2---configura��o-do-programcs)
   3. [Etapa 3 - Prote��o de Endpoints](#etapa-3---prote��o-de-endpoints)
   4. [Etapa 4 - Gerar Token de Autoriza��o (Opcional)](#etapa-4---gerar-token-de-autoriza��o-opcional)
2. [Parte 2 - Implementa��o de Identity](#parte-2---implementa��o-de-identity)
   1. [Etapa 1 - Instalar Pacote Identity](#etapa-1---instalar-pacote-identity)
   2. [Etapa 2 - Modificar DbContext](#etapa-2---modificar-dbcontext)
   3. [Etapa 3 - Ajuste no Program.cs](#etapa-3---ajuste-no-programcs)
   4. [Etapa 4 - Aplicar Migrations](#etapa-4---aplicar-migrations)
3. [Parte 3 - Configura��o de JWT Bearer](#parte-3---configura��o-de-jwt-bearer)
   1. [Etapa 1 - Instalar Pacotes JWT Bearer](#etapa-1---instalar-pacotes-jwt-bearer)
   2. [Etapa 2 - Ajustar appsettings.json](#etapa-2---ajustar-appsettingsjson)
   3. [Etapa 3 - Configurar Program.cs](#etapa-3---configurar-programcs)
4. [Parte 4 - Implementa��o de Refresh Token](#parte-4---implementa��o-de-refresh-token)
   1. [Etapa 1 - Criar Classe ApplicationUser](#etapa-1---criar-classe-applicationuser)
   2. [Etapa 2 - Ajustar DbContext](#etapa-2---ajustar-dbcontext)
   3. [Etapa 3 - Ajuste no Program.cs](#etapa-3---ajuste-no-programcs)
5. [Parte 5 - Cria��o de DTOs](#parte-5---cria��o-de-dtos)
   1. [Etapa 1 - DTO de Login](#etapa-1---dto-de-login)
   2. [Etapa 2 - DTO de Registro](#etapa-2---dto-de-registro)
   3. [Etapa 3 - Model de Token](#etapa-3---model-de-token)
   4. [Etapa 4 - Response Model](#etapa-4---response-model)
6. [Parte 6 - Gera��o de Tokens](#parte-6---gera��o-de-tokens)
   1. [Etapa 1 - Criar Interface ITokenService](#etapa-1---criar-interface-itokenservice)
   2. [Etapa 2 - Implementa��o da Interface ITokenService](#etapa-2---implementa��o-da-interface-itokenservice)
   3. [Etapa 3 - Ajuste no Program.cs](#etapa-3---ajuste-no-programcs)
7. [Resumo Final](#resumo-final)

---

## **Parte 1 - Configura��o Inicial de JWT**

### **Etapa 1 - Instalar Pacote JWT Bearer**

Para iniciar, precisamos instalar o pacote necess�rio para autentica��o JWT Bearer. Execute o seguinte comando no terminal:

```bash
dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer
```

### **Etapa 2 - Configura��o do Program.cs**

Agora, vamos configurar o `Program.cs` para adicionar a autentica��o e autoriza��o baseadas em JWT. Isso inclui adicionar os servi�os ao container e configurar o middleware para autentica��o JWT.

```csharp
// Adiciona os servi�os de autentica��o e autoriza��o ao container
builder.Services.AddAuthorization();
builder.Services.AddAuthentication("bearer").AddJwtBearer();

// Pode remover a linha abaixo se a autoriza��o n�o for necess�ria em toda a aplica��o
app.UseAuthorization();
```

### **Etapa 3 - Prote��o de Endpoints**

Com a autentica��o configurada, podemos proteger nossos endpoints utilizando o atributo `[Authorize]`. Aqui est� um exemplo de como aplicar essa prote��o a um m�todo espec�fico:

```csharp
[HttpGet("{id:int}", Name = "ObterAluno")]
[Authorize] // Este atributo garante que apenas usu�rios autenticados possam acessar este endpoint
public async Task<ActionResult<AlunoDto>> GetAluno(int id)
{
    var aluno = await _unitOfWork.AlunoRepository.GetAsync(a => a.AlunoId == id);
    if (aluno == null) { NotFound(); }
    var alunoDto = _mapper.Map<AlunoDto>(aluno);
    return Ok(alunoDto);
}
```

### **Etapa 4 - Gerar Token de Autoriza��o (Opcional)**

Se desejar, voc� pode gerar e gerenciar tokens JWT usando a ferramenta `dotnet user-jwt`. Para criar um token, utilize o seguinte comando:

```bash
dotnet user-jwt create
```

#### **Comandos Dispon�veis:**

- **add**: Adiciona um token JWT ao projeto.
- **remove**: Remove um token JWT do projeto.
- **list**: Lista todos os tokens JWT associados ao projeto.
- **clear**: Remove todos os tokens JWT do projeto.
- **print**: Imprime as informa��es detalhadas de um token JWT espec�fico.

Ap�s gerar o token, voc� pode utiliz�-lo para acessar os endpoints protegidos.

---

## **Parte 2 - Implementa��o de Identity**

### **Etapa 1 - Instalar Pacote Identity**

Para come�ar a trabalhar com Identity, voc� precisa instalar o pacote `Microsoft.AspNetCore.Identity.EntityFrameworkCore`:

```bash
dotnet add package Microsoft.AspNetCore.Identity.EntityFrameworkCore
```

### **Etapa 2 - Modificar DbContext**

Altere a classe `AppDbContext` para herdar de `IdentityDbContext`. Isso permitir� que as tabelas necess�rias para gerenciar usu�rios e tokens sejam criadas automaticamente:

```csharp
public class AppDbContext : IdentityDbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    // Adicione suas DbSets aqui
    public DbSet<Disciplina>? Disciplinas { get; set; }
    public DbSet<Aluno>? Alunos { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        // Configura��es adicionais
    }
}
```

### **Etapa 3 - Ajuste no Program.cs**

No arquivo `Program.cs`, configure o servi�o de Identity:

```csharp
builder.Services.AddIdentity<IdentityUser, IdentityRole>()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();
```

### **Etapa 4 - Aplicar Migrations**

Depois de configurar o `DbContext`, aplique as migrations para criar as tabelas necess�rias no banco de dados:

```bash
dotnet ef migrations add InitialIdentitySchema
dotnet ef database update
```

---

## **Parte 3 - Configura��o de JWT Bearer**

### **Etapa 1 - Instalar Pacotes JWT Bearer**

Instale o pacote de autentica��o JWT Bearer se ainda n�o o fez:

```bash
dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer
```

### **Etapa 2 - Ajustar appsettings.json**

No arquivo `appsettings.json`, adicione a seguinte configura��o para JWT:

```json
"Jwt": {
    "ValidAudience": "http://localhost:7066",
    "ValidIssuer": "http//localhost:5066",
    "SecretKey": "Minha@Super#Secreta&Chave*Priavada!2024%",
    "TokenValidityInMinutes": 30,
    "RefreshTokenValidityInMinutes": 60
}
```

> **Nota:** N�o use a chave secreta diretamente no c�digo em produ��o. Utilize um gerenciador de segredos.

### **Etapa 3 - Configurar Program.cs**

Configure o `Program.cs` para utilizar as configura��es de JWT:

```csharp
var secretKey = builder.Configuration["JWT:SecretKey"]
               ?? throw new ArgumentException("Invalid secret key!!");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.SaveToken = true;
    options.RequireHttpsMetadata = false; // Em produção vc ponhe true 
    options.TokenValidationParameters = new TokenValidationParameters()
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ClockSkew = TimeSpan.Zero,
        ValidAudience = builder.Configuration["JWT:ValidAudience"],
        ValidIssuer = builder.Configuration["JWT:ValidIssuer"],
        IssuerSigningKey = new SymmetricSecurityKey

(
                           Encoding.UTF8.GetBytes(secretKey))
    };
});
```

---

## **Parte 4 - Implementa��o de Refresh Token**

### **Etapa 1 - Criar Classe ApplicationUser**

Crie a classe `ApplicationUser`, que ser� utilizada para gerenciar os Refresh Tokens.

```csharp
using Microsoft.AspNetCore.Identity;

namespace Curso.Api.Models
{
    public class ApplicationUser : IdentityUser
    {
        // Propriedade para armazenar o Refresh Token
        public string? RefreshToken { get; set; }

        // Data de expira��o do Refresh Token
        public DateTime RefreshTokenExpiryTime { get; set; }                                        
    }
}
```

### **Etapa 2 - Ajustar DbContext**

Altere o `AppDbContext` para herdar de `IdentityDbContext<ApplicationUser>`. Isso permite que a classe `ApplicationUser` seja utilizada para gerenciamento de tokens:

```csharp
public class AppDbContext : IdentityDbContext<ApplicationUser>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    // Adicione suas DbSets aqui
    public DbSet<Disciplina>? Disciplinas { get; set; }
    public DbSet<Aluno>? Alunos { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        // Configura��es adicionais
    }
}
```

### **Etapa 3 - Ajuste no Program.cs**

No `Program.cs`, adicione a configura��o para Identity com a classe `ApplicationUser`:

```csharp
builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();
```

---

## **Parte 5 - Cria��o de DTOs**

### **Etapa 1 - DTO de Login**

Crie um DTO para login, que ser� utilizado para transferir os dados de login do usu�rio:

```csharp
public class LoginDto
{
    public string? Email { get; set; }
    public string? Password { get; set; }
}
```

### **Etapa 2 - DTO de Registro**

Crie um DTO para registro, utilizado ao registrar novos usu�rios:

```csharp
public class RegisterDto
{
    public string? Email { get; set; }
    public string? Password { get; set; }
    public string? ConfirmPassword { get; set; }
}
```

### **Etapa 3 - Model de Token**

Crie um modelo para o Token, que ser� utilizado para transferir informa��es do token:

```csharp
public class TokenModel
{
    public string? AccessToken { get; set; }
    public string? RefreshToken { get; set; }
}
```

### **Etapa 4 - Response Model**

Crie um modelo de resposta que encapsula a resposta da API com tokens:

```csharp
public class AuthResponseModel
{
    public string? Token { get; set; }
    public string? RefreshToken { get; set; }
    public DateTime Expiration { get; set; }
}
```

---

## **Parte 6 - Gera��o de Tokens**

### **Etapa 1 - Criar Interface ITokenService**

Crie a interface `ITokenService` para definir os m�todos que ser�o implementados no servi�o de gera��o de tokens:

```csharp
public interface ITokenService
{
    Task<string> CreateTokenAsync(ApplicationUser user);
    Task<string> CreateRefreshTokenAsync();
}
```

### **Etapa 2 - Implementa��o da Interface ITokenService**

Implemente a interface `ITokenService`:

```csharp
public class TokenService : ITokenService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IConfiguration _configuration;

    public TokenService(UserManager<ApplicationUser> userManager, IConfiguration configuration)
    {
        _userManager = userManager;
        _configuration = configuration;
    }

    public async Task<string> CreateTokenAsync(ApplicationUser user)
    {
        var authClaims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, user.UserName ?? string.Empty),
            new Claim(ClaimTypes.Email, user.Email ?? string.Empty),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:SecretKey"]));

        var token = new JwtSecurityToken(
            issuer: _configuration["JWT:ValidIssuer"],
            audience: _configuration["JWT:ValidAudience"],
            expires: DateTime.Now.AddMinutes(Convert.ToDouble(_configuration["JWT:TokenValidityInMinutes"])),
            claims: authClaims,
            signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public Task<string> CreateRefreshTokenAsync()
    {
        var randomNumber = new byte[32];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(randomNumber);
            return Task.FromResult(Convert.ToBase64String(randomNumber));
        }
    }
}
```

### **Etapa 3 - Ajuste no Program.cs**

No `Program.cs`, registre o servi�o de tokens no container de inje��o de depend�ncias:

```csharp
builder.Services.AddScoped<ITokenService, TokenService>();
```

---

## **Resumo Final**

Neste guia, implementamos autentica��o JWT, Identity e Refresh Tokens em uma aplica��o ASP.NET Core. A configura��o abrange desde a instala��o de pacotes at� a cria��o de DTOs e servi�os para gerenciar a autentica��o de usu�rios. Este setup proporciona uma base s�lida para aplica��es que requerem autentica��o segura e escal�vel, al�m de fornecer uma maneira eficiente de gerenciar tokens de acesso e Refresh Tokens.

--- 

Este tutorial oferece uma vis�o completa e detalhada do processo, garantindo que todos os aspectos da implementa��o de JWT e Identity sejam cobertos e compreendidos.