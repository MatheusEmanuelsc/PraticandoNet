
---

### **Implementação de JWT, Identity e Refresh Tokens no ASP.NET Core**

Este guia fornece um passo a passo para configurar autenticação JWT, Identity e Refresh Tokens em uma aplicação ASP.NET Core. O tutorial está dividido em várias partes, cada uma abordando uma etapa específica da implementação.

## **Índice**

1. [Parte 1 - Configuração Inicial de JWT](#parte-1---configuração-inicial-de-jwt)
   1. [Etapa 1 - Instalar Pacote JWT Bearer](#etapa-1---instalar-pacote-jwt-bearer)
   2. [Etapa 2 - Configuração do Program.cs](#etapa-2---configuração-do-programcs)
   3. [Etapa 3 - Proteção de Endpoints](#etapa-3---proteção-de-endpoints)
   4. [Etapa 4 - Gerar Token de Autorização (Opcional)](#etapa-4---gerar-token-de-autorização-opcional)
2. [Parte 2 - Implementação de Identity](#parte-2---implementação-de-identity)
   1. [Etapa 1 - Instalar Pacote Identity](#etapa-1---instalar-pacote-identity)
   2. [Etapa 2 - Modificar DbContext](#etapa-2---modificar-dbcontext)
   3. [Etapa 3 - Ajuste no Program.cs](#etapa-3---ajuste-no-programcs)
   4. [Etapa 4 - Aplicar Migrations](#etapa-4---aplicar-migrations)
3. [Parte 3 - Configuração de JWT Bearer](#parte-3---configuração-de-jwt-bearer)
   1. [Etapa 1 - Instalar Pacotes JWT Bearer](#etapa-1---instalar-pacotes-jwt-bearer)
   2. [Etapa 2 - Ajustar appsettings.json](#etapa-2---ajustar-appsettingsjson)
   3. [Etapa 3 - Configurar Program.cs](#etapa-3---configurar-programcs)
4. [Parte 4 - Implementação de Refresh Token](#parte-4---implementação-de-refresh-token)
   1. [Etapa 1 - Criar Classe ApplicationUser](#etapa-1---criar-classe-applicationuser)
   2. [Etapa 2 - Ajustar DbContext](#etapa-2---ajustar-dbcontext)
   3. [Etapa 3 - Ajuste no Program.cs](#etapa-3---ajuste-no-programcs)
5. [Parte 5 - Criação de DTOs](#parte-5---criação-de-dtos)
   1. [Etapa 1 - DTO de Login](#etapa-1---dto-de-login)
   2. [Etapa 2 - DTO de Registro](#etapa-2---dto-de-registro)
   3. [Etapa 3 - Model de Token](#etapa-3---model-de-token)
   4. [Etapa 4 - Response Model](#etapa-4---response-model)
6. [Parte 6 - Geração de Tokens](#parte-6---geração-de-tokens)
   1. [Etapa 1 - Criar Interface ITokenService](#etapa-1---criar-interface-itokenservice)
   2. [Etapa 2 - Implementação da Interface ITokenService](#etapa-2---implementação-da-interface-itokenservice)
   3. [Etapa 3 - Ajuste no Program.cs](#etapa-3---ajuste-no-programcs)
7. [Resumo Final](#resumo-final)

---

## **Parte 1 - Configuração Inicial de JWT**

### **Etapa 1 - Instalar Pacote JWT Bearer**

Para iniciar, precisamos instalar o pacote necessário para autenticação JWT Bearer. Execute o seguinte comando no terminal:

```bash
dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer
```

### **Etapa 2 - Configuração do Program.cs**

Agora, vamos configurar o `Program.cs` para adicionar a autenticação e autorização baseadas em JWT. Isso inclui adicionar os serviços ao container e configurar o middleware para autenticação JWT.

```csharp
// Adiciona os serviços de autenticação e autorização ao container
builder.Services.AddAuthorization();
builder.Services.AddAuthentication("bearer").AddJwtBearer();

// Pode remover a linha abaixo se a autorização não for necessária em toda a aplicação
app.UseAuthorization();
```

### **Etapa 3 - Proteção de Endpoints**

Com a autenticação configurada, podemos proteger nossos endpoints utilizando o atributo `[Authorize]`. Aqui está um exemplo de como aplicar essa proteção a um método específico:

```csharp
[HttpGet("{id:int}", Name = "ObterAluno")]
[Authorize] // Este atributo garante que apenas usuários autenticados possam acessar este endpoint
public async Task<ActionResult<AlunoDto>> GetAluno(int id)
{
    var aluno = await _unitOfWork.AlunoRepository.GetAsync(a => a.AlunoId == id);
    if (aluno == null) { NotFound(); }
    var alunoDto = _mapper.Map<AlunoDto>(aluno);
    return Ok(alunoDto);
}
```

### **Etapa 4 - Gerar Token de Autorização (Opcional)**

Se desejar, você pode gerar e gerenciar tokens JWT usando a ferramenta `dotnet user-jwt`. Para criar um token, utilize o seguinte comando:

```bash
dotnet user-jwt create
```

#### **Comandos Disponíveis:**

- **add**: Adiciona um token JWT ao projeto.
- **remove**: Remove um token JWT do projeto.
- **list**: Lista todos os tokens JWT associados ao projeto.
- **clear**: Remove todos os tokens JWT do projeto.
- **print**: Imprime as informações detalhadas de um token JWT específico.

Após gerar o token, você pode utilizá-lo para acessar os endpoints protegidos.

---

## **Parte 2 - Implementação de Identity**

### **Etapa 1 - Instalar Pacote Identity**

Para começar a trabalhar com Identity, você precisa instalar o pacote `Microsoft.AspNetCore.Identity.EntityFrameworkCore`:

```bash
dotnet add package Microsoft.AspNetCore.Identity.EntityFrameworkCore
```

### **Etapa 2 - Modificar DbContext**

Altere a classe `AppDbContext` para herdar de `IdentityDbContext`. Isso permitirá que as tabelas necessárias para gerenciar usuários e tokens sejam criadas automaticamente:

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
        // Configurações adicionais
    }
}
```

### **Etapa 3 - Ajuste no Program.cs**

No arquivo `Program.cs`, configure o serviço de Identity:

```csharp
builder.Services.AddIdentity<IdentityUser, IdentityRole>()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();
```

### **Etapa 4 - Aplicar Migrations**

Depois de configurar o `DbContext`, aplique as migrations para criar as tabelas necessárias no banco de dados:

```bash
dotnet ef migrations add InitialIdentitySchema
dotnet ef database update
```

---

## **Parte 3 - Configuração de JWT Bearer**

### **Etapa 1 - Instalar Pacotes JWT Bearer**

Instale o pacote de autenticação JWT Bearer se ainda não o fez:

```bash
dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer
```

### **Etapa 2 - Ajustar appsettings.json**

No arquivo `appsettings.json`, adicione a seguinte configuração para JWT:

```json
"Jwt": {
    "ValidAudience": "http://localhost:7066",
    "ValidIssuer": "http//localhost:5066",
    "SecretKey": "Minha@Super#Secreta&Chave*Priavada!2024%",
    "TokenValidityInMinutes": 30,
    "RefreshTokenValidityInMinutes": 60
}
```

> **Nota:** Não use a chave secreta diretamente no código em produção. Utilize um gerenciador de segredos.

### **Etapa 3 - Configurar Program.cs**

Configure o `Program.cs` para utilizar as configurações de JWT:

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
    options.RequireHttpsMetadata = false;
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

## **Parte 4 - Implementação de Refresh Token**

### **Etapa 1 - Criar Classe ApplicationUser**

Crie a classe `ApplicationUser`, que será utilizada para gerenciar os Refresh Tokens.

```csharp
using Microsoft.AspNetCore.Identity;

namespace Curso.Api.Models
{
    public class ApplicationUser : IdentityUser
    {
        // Propriedade para armazenar o Refresh Token
        public string? RefreshToken { get; set; }

        // Data de expiração do Refresh Token
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
        // Configurações adicionais
    }
}
```

### **Etapa 3 - Ajuste no Program.cs**

No `Program.cs`, adicione a configuração para Identity com a classe `ApplicationUser`:

```csharp
builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();
```

---

## **Parte 5 - Criação de DTOs**

### **Etapa 1 - DTO de Login**

Crie um DTO para login, que será utilizado para transferir os dados de login do usuário:

```csharp
public class LoginDto
{
    public string? Email { get; set; }
    public string? Password { get; set; }
}
```

### **Etapa 2 - DTO de Registro**

Crie um DTO para registro, utilizado ao registrar novos usuários:

```csharp
public class RegisterDto
{
    public string? Email { get; set; }
    public string? Password { get; set; }
    public string? ConfirmPassword { get; set; }
}
```

### **Etapa 3 - Model de Token**

Crie um modelo para o Token, que será utilizado para transferir informações do token:

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

## **Parte 6 - Geração de Tokens**

### **Etapa 1 - Criar Interface ITokenService**

Crie a interface `ITokenService` para definir os métodos que serão implementados no serviço de geração de tokens:

```csharp
public interface ITokenService
{
    Task<string> CreateTokenAsync(ApplicationUser user);
    Task<string> CreateRefreshTokenAsync();
}
```

### **Etapa 2 - Implementação da Interface ITokenService**

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

No `Program.cs`, registre o serviço de tokens no container de injeção de dependências:

```csharp
builder.Services.AddScoped<ITokenService, TokenService>();
```

---

## **Resumo Final**

Neste guia, implementamos autenticação JWT, Identity e Refresh Tokens em uma aplicação ASP.NET Core. A configuração abrange desde a instalação de pacotes até a criação de DTOs e serviços para gerenciar a autenticação de usuários. Este setup proporciona uma base sólida para aplicações que requerem autenticação segura e escalável, além de fornecer uma maneira eficiente de gerenciar tokens de acesso e Refresh Tokens.

--- 

Este tutorial oferece uma visão completa e detalhada do processo, garantindo que todos os aspectos da implementação de JWT e Identity sejam cobertos e compreendidos.