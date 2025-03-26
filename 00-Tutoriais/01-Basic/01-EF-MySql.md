
```markdown
# Utilizando o Entity Framework com MySQL (Pomelo)

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
Para utilizar o Entity Framework Core com MySQL via Pomelo, instale o seguinte pacote NuGet essencial:
- **`Pomelo.EntityFrameworkCore.MySql`**  
  Este é o provedor principal para integração do EF Core com MySQL. Ele inclui tudo o que é necessário para runtime (execução da aplicação).

**Pacote Condicional:**
- **`Microsoft.EntityFrameworkCore.Design`**  
  Necessário apenas se você estiver fora do Visual Studio (ex.: Linux, VS Code) e precisar usar o CLI (`dotnet ef`) para tarefas de design-time, como criar migrações ou gerar código a partir de um banco existente. Não é necessário para runtime.

**Nota:** O pacote `Microsoft.EntityFrameworkCore.Tools` não é essencial fora do Visual Studio, pois é voltado para o Package Manager Console. Para CLI, o `Microsoft.EntityFrameworkCore.Design` é suficiente com o `dotnet ef`.

### 1.2. Versões Recomendadas
Use versões compatíveis com seu projeto. Para .NET 8, por exemplo, instale a versão mais recente do `Pomelo.EntityFrameworkCore.MySql` (ex.: 8.0.3, a partir de março de 2025). Verifique compatibilidade no NuGet ou no repositório oficial do Pomelo.

---

## 2. Configurando a String de Conexão

### 2.1. Estrutura da String de Conexão
A string de conexão define a conexão com o MySQL. Um exemplo com autenticação por usuário e senha (funcional em qualquer ambiente, como Linux):
```
Server=NOME_DO_SERVIDOR;Database=NOME_DO_BANCO;User Id=USUARIO;Password=SENHA;
```
- **`Server`**: Nome ou IP do servidor (ex.: `localhost`, `192.168.1.100`).
- **`Database`**: Nome do banco de dados.
- **`User Id`**: Usuário do MySQL.
- **`Password`**: Senha correspondente.

### 2.2. Armazenamento Seguro da String
Armazene a string no arquivo `appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=MeuBanco;User Id=root;Password=minhasenha;"
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
Configure o MySQL com Pomelo no método `ConfigureServices` (ex.: em `Program.cs`):
```csharp
builder.Services.AddDbContext<MeuContexto>(options =>
    options.UseMySql(builder.Configuration.GetConnectionString("DefaultConnection"), 
        ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("DefaultConnection"))));
```
- **`UseMySql`**: Método do Pomelo para configurar o MySQL.
- **`ServerVersion.AutoDetect`**: Detecta automaticamente a versão do MySQL a partir da string de conexão.

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

### Nota sobre Uso Fora do Visual Studio
Se você estiver fora do Visual Studio e precisar usar o CLI (`dotnet ef`):
1. Instale o CLI globalmente:
   ```
   dotnet tool install --global dotnet-ef
   ```
2. Adicione o pacote `Microsoft.EntityFrameworkCore.Design`:
   ```
   dotnet add package Microsoft.EntityFrameworkCore.Design
   ```
3. Execute comandos como:
   ```
   dotnet ef migrations add InitialCreate
   dotnet ef database update
   ```

O `Pomelo.EntityFrameworkCore.MySql` é suficiente para runtime, e o `Microsoft.EntityFrameworkCore.Design` cobre as necessidades de design-time no CLI.

---

