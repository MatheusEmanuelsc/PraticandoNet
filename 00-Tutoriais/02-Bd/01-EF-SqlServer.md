

```markdown
# Utilizando o Entity Framework com SQL Server

## Índice
1. [Pacotes Necessários](#pacotes-necessários)  
   1.1. [Pacotes NuGet Essenciais](#pacotes-nuget-essenciais)  
   1.2. [Versões Recomendadas](#versões-recomendadas)  
2. [Configurando a String de Conexão](#configurando-a-string-de-conexão)  
   2.1. [Estrutura da String de Conexão](#estrutura-da-string-de-conexão)  
   2.2. [Armazenamento Seguro da String](#armazenamento-seguro-da-string)  
3. [Criando a Classe de Contexto](#criando-a-classe-de-contexto)  
   3.1. [Estrutura Básica da Classe](#estrutura-básica-da-classe)  
   3.2. [Configuração do DbContext](#configuração-do-dbcontext)  
   3.3. [Definindo DbSets](#definindo-dbsets)  

---

## 1. Pacotes Necessários

### 1.1. Pacotes NuGet Essenciais
Para utilizar o Entity Framework com SQL Server, instale os seguintes pacotes NuGet:
- **`Microsoft.EntityFrameworkCore.SqlServer`**  
  Provedor específico para integração com o SQL Server.
- **`Microsoft.EntityFrameworkCore.Tools`** (opcional)  
  Útil para comandos de migração e scaffolding via CLI.
- **`Microsoft.EntityFrameworkCore.Design`** (condicional)  
  Necessário apenas se você estiver fora do Visual Studio (ex.: usando CLI em outro ambiente) e precisar executar comandos como `dotnet ef migrations` ou `dotnet ef dbcontext scaffold`. Não é requerido para runtime, apenas para design-time.

### 1.2. Versões Recomendadas
Use versões compatíveis com seu projeto. Para .NET 8, por exemplo, prefira a versão mais recente do EF Core (ex.: 8.x.x). Verifique compatibilidade no NuGet ou na documentação oficial.

---

## 2. Configurando a String de Conexão

### 2.1. Estrutura da String de Conexão
A string de conexão define a conexão com o SQL Server. Como você está usando `localhost`, aqui está um exemplo com autenticação SQL:
```
Server=localhost;Database=NOME_DO_BANCO;User Id=USUARIO;Password=SENHA;
```
- **`Server`**: `localhost` para o servidor local.
- **`Database`**: Nome do banco de dados.
- **`User Id`**: Usuário do SQL Server (ex.: `sa` para o usuário padrão).
- **`Password`**: Senha correspondente.

### 2.2. Armazenamento Seguro da String
Armazene a string no arquivo `appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=MeuBanco;User Id=sa;Password=minhasenha;"
  }
}
```
Acesse no código com `IConfiguration` via injeção de dependência.

---

## 3. Criando a Classe de Contexto

### 3.1. Estrutura Básica da Classe
A classe de contexto herda de `DbContext` e gerencia a conexão com o banco:
```csharp
using Microsoft.EntityFrameworkCore;

public class MeuContexto : DbContext
{
    public MeuContexto(DbContextOptions<MeuContexto> options) : base(options)
    {
    }
}
```

### 3.2. Configuração do DbContext
Configure o SQL Server no método `ConfigureServices` (ex.: em `Program.cs`):
```csharp
builder.Services.AddDbContext<MeuContexto>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
```

### 3.3. Definindo DbSets
Adicione propriedades `DbSet` para representar as tabelas:
```csharp
public class MeuContexto : DbContext
{
    public MeuContexto(DbContextOptions<MeuContexto> options) : base(options)
    {
    }

    public DbSet<Cliente> Clientes { get; set; }
    public DbSet<Pedido> Pedidos { get; set; }
}

public class Cliente
{
    public int Id { get; set; }
    public string Nome { get; set; }
}

public class Pedido
{
    public int Id { get; set; }
    public int ClienteId { get; set; }
}
```

---

### Nota sobre `Microsoft.EntityFrameworkCore.Design`
O pacote `Microsoft.EntityFrameworkCore.Design` é necessário apenas em cenários fora do Visual Studio, como ao usar o CLI (`dotnet ef`) para criar migrações ou gerar código a partir de um banco existente. Se você só vai rodar a aplicação (runtime), ele não é necessário. Inclua-o se planeja usar comandos de design-time:
```
dotnet add package Microsoft.EntityFrameworkCore.Design
```
E instale o CLI globalmente, se necessário:
```
dotnet tool install --global dotnet-ef
```

---

