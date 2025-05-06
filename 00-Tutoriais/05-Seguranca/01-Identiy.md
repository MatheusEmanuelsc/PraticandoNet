# 📄 Tutorial: ASP.NET Core Identity em Projetos com Unit of Work e Repository Pattern

## Sumário
1. [Visão Geral](#visão-geral)
2. [Pré-requisitos](#pré-requisitos)
3. [Implementação do ASP.NET Core Identity](#implementação-do-aspnet-core-identity)
   - [Instalação de Pacotes](#instalação-de-pacotes)
   - [Criação do ApplicationUser](#criação-do-applicationuser)
   - [Configuração do IdentityDbContext](#configuração-do-identitydbcontext)
   - [Configuração no Program.cs](#configuração-no-programcs)
4. [Gerenciamento de Múltiplos DbContexts](#gerenciamento-de-múltiplos-dbcontexts)
   - [Configuração de Strings de Conexão](#configuração-de-strings-de-conexão)
   - [Migrações Específicas para Identity](#migrações-específicas-para-identity)
5. [Melhores Práticas e Considerações](#melhores-práticas-e-considerações)
6. [Próximos Passos](#próximos-passos)

## Visão Geral

O ASP.NET Core Identity é um sistema completo de gerenciamento de identidade que permite implementar funcionalidades de autenticação e autorização em aplicações .NET. Este tutorial explica como integrar o Identity em projetos que já utilizam padrões arquiteturais como Unit of Work e Repository, mantendo a separação de contextos de banco de dados para uma melhor organização do código.

A abordagem apresentada separa claramente o contexto de identidade do contexto de domínio da aplicação, proporcionando maior flexibilidade e escalabilidade ao projeto.

## Pré-requisitos

- Projeto ASP.NET Core 8
- Entity Framework Core configurado
- Conhecimento básico em:
  - ASP.NET Core e sua pipeline de requisições
  - Entity Framework Core e migrações
  - Padrões Unit of Work e Repository
  - Conceitos fundamentais de autenticação e autorização

## Implementação do ASP.NET Core Identity

### Instalação de Pacotes

Primeiro, adicione os pacotes NuGet necessários ao seu projeto:

```bash
# Pacotes essenciais para o Identity com Entity Framework
dotnet add package Microsoft.AspNetCore.Identity.EntityFrameworkCore

# Para autenticação via JWT (preparando para etapas futuras)
dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer

# Opcional: Se desejar usar interfaces de usuário pré-construídas
dotnet add package Microsoft.AspNetCore.Identity.UI
```

### Criação do ApplicationUser

Crie uma classe personalizada estendendo o `IdentityUser` para adicionar propriedades específicas da sua aplicação:

```csharp
// Models/Identity/ApplicationUser.cs
using Microsoft.AspNetCore.Identity;

namespace YourNamespace.Models.Identity
{
    public class ApplicationUser : IdentityUser
    {
        // Propriedades personalizadas
        public string FullName { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; } = true;
        
        // Adicione outras propriedades relevantes para seu domínio
    }
}
```

Esta classe permite estender o modelo padrão de usuário com informações adicionais específicas para seu sistema.

### Configuração do IdentityDbContext

Crie um contexto de banco de dados separado dedicado ao Identity:

```csharp
// Data/Identity/IdentityContext.cs
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using YourNamespace.Models.Identity;

namespace YourNamespace.Data.Identity
{
    public class IdentityContext : IdentityDbContext<ApplicationUser>
    {
        public IdentityContext(DbContextOptions<IdentityContext> options) : base(options)
        {
        }
        
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            
            // Personalizações adicionais do modelo, se necessário
            // Ex: builder.Entity<ApplicationUser>().ToTable("Users");
        }
    }
}
```

### Configuração no Program.cs

Configure o Identity no arquivo `Program.cs` da aplicação:

```csharp
// Program.cs
var builder = WebApplication.CreateBuilder(args);

// Configuração do contexto de Identity
builder.Services.AddDbContext<IdentityContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("IdentityConnection")));

// Configuração do contexto de domínio (existente)
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DomainConnection")));

// Configuração do sistema Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options => {
    // Políticas de senha
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 8;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = true;
    
    // Políticas de usuário
    options.User.RequireUniqueEmail = true;
    options.SignIn.RequireConfirmedEmail = true;
    
    // Bloqueio de conta
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
    options.Lockout.MaxFailedAccessAttempts = 5;
})
.AddEntityFrameworkStores<IdentityContext>()
.AddDefaultTokenProviders();

// Resto da configuração da aplicação...
```

## Gerenciamento de Múltiplos DbContexts

### Configuração de Strings de Conexão

No arquivo `appsettings.json`, defina conexões separadas para cada contexto:

```json
{
  "ConnectionStrings": {
    "DomainConnection": "Server=localhost;Database=YourAppDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True",
    "IdentityConnection": "Server=localhost;Database=YourIdentityDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
  }
}
```

Esta separação permite escalar cada banco de dados independentemente e facilita a manutenção.

### Migrações Específicas para Identity

Execute comandos específicos para criar e aplicar migrações do contexto de Identity:

```bash
# Criar a migração inicial do Identity
dotnet ef migrations add InitialIdentitySetup --context IdentityContext --output-dir Data/Migrations/Identity

# Aplicar a migração ao banco de dados
dotnet ef database update --context IdentityContext
```

Para o contexto de domínio, faça o mesmo processo com o contexto apropriado:

```bash
# Criar migração do contexto de domínio (se necessário)
dotnet ef migrations add YourMigrationName --context ApplicationDbContext --output-dir Data/Migrations/Domain

# Aplicar a migração
dotnet ef database update --context ApplicationDbContext
```

## Melhores Práticas e Considerações

1. **Separação de Responsabilidades**:
   - Mantenha os contextos de Identity e domínio completamente separados
   - Evite referências cruzadas entre os modelos de domínio e de identidade

2. **Segurança**:
   - Configure políticas de senha adequadas ao seu cenário
   - Implemente confirmação de email para maior segurança
   - Considere autenticação de dois fatores para sistemas críticos

3. **Persistência**:
   - Avalie se os bancos precisam estar fisicamente separados ou podem compartilhar o mesmo servidor
   - Em ambientes de produção, considere estratégias de backup diferentes para cada contexto

4. **Organização do Código**:
   - Agrupe classes relacionadas ao Identity em namespaces dedicados
   - Crie serviços específicos para operações de identidade, evitando lógica de autenticação em controllers

5. **Performance**:
   - Use índices apropriados nas tabelas do Identity
   - Considere implementar cache para dados de identidade frequentemente acessados

## Próximos Passos

Com o Identity configurado, você pode prosseguir para:

1. Implementar autenticação JWT para APIs
2. Criar controllers de conta (registro, login, recuperação de senha)
3. Implementar autorização baseada em roles e policies
4. Adicionar autenticação externa (Google, Facebook, etc.)
5. Implementar recursos avançados como autenticação de dois fatores

Cada um desses tópicos merece seu próprio tutorial detalhado para uma implementação completa e segura.