## ORMs Mais Populares: Entity Framework (EF) em Detalhes

**Introdução:**

Neste guia, vamos explorar o Entity Framework (EF), um dos ORMs (Object-Relational Mappers) mais populares para .NET. O EF facilita a interação entre aplicativos .NET e bancos de dados relacionais, mapeando objetos em código para tabelas e linhas no banco de dados.

**Pacotes Necessários:**

Para utilizar o EF, você precisa instalar dois pacotes NuGet:

1. `Microsoft.EntityFrameworkCore`: Este pacote fornece o núcleo do EF, incluindo classes e funcionalidades para mapeamento de objetos, consultas e operações de banco de dados.
2. `Microsoft.EntityFrameworkCore.Tools`: Este pacote fornece ferramentas de linha de comando para gerenciar migrações de banco de dados, que discutiremos mais adiante.

**Instalação das Ferramentas:**

As ferramentas do EF ainda estão em desenvolvimento, então, para usá-las completamente, você precisa instalar um complemento:

```bash
dotnet tool install -g dotnet-ef
```

Este comando instala as ferramentas do EF globalmente. Se precisar de uma versão específica, desinstale e instale novamente com a versão desejada.

**Driver do Banco de Dados:**

Para se conectar ao seu banco de dados (MySQL, SQL Server, SQLite ou outro), você precisa instalar o driver específico. Por exemplo, para SQLite:

```bash
dotnet add package Microsoft.EntityFrameworkCore.Sqlite
```

**Migrações:**

As migrações são arquivos de código que registram as alterações no seu modelo de dados e geram as instruções SQL necessárias para atualizar o banco de dados de acordo. Elas permitem o versionamento do seu banco de dados, semelhante ao Git.

**DbContext:**

A classe `DbContext` é o coração do EF. Ela representa a conexão com o banco de dados e fornece métodos para realizar operações como consultas, inserções, atualizações e exclusões.

**DbSet:**

A propriedade `DbSet<T>` em `DbContext` representa uma tabela específica no banco de dados. Através dela, você pode acessar e manipular os dados da tabela como objetos em seu código.

**Exemplo de Criação de Tabela:**

```c#
public class AppDBContext : DbContext
{
    // Tabela
    public DbSet<Atendimento> Atendimentos { get; set; }

    public AppDBContext(DbContextOptions<AppDBContext> options) : base(options)
    {
    }
}
```

**Importação de Namespaces:**

Lembre-se de importar os namespaces corretos no seu código:

```c#
using Microsoft.EntityFrameworkCore;
using _04_Database.Domain.Model; // Assumindo que Atendimento está neste namespace
```

**Configuração da Connection String:**

No arquivo `appsettings.json`, defina a connection string para o seu banco de dados:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=atendimentos.db"
  }
}
```

**Adicionar DbContext no Program.cs:**

No `Program.cs`, configure o EF no seu aplicativo:

```c#
builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection"));
});
```
mysql
"ConnectionStrings": {
  "DefaultConnection": "server=localhost;DataBase=CatalogoDB;Uid=root;Pwd="senha aqui"
},
var mySqlConnection = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseMySql(mySqlConnection, ServerVersion.AutoDetect(mySqlConnection));
});

**Criando Migrações:**

Para gerar as migrações iniciais, execute o seguinte comando:

```bash
dotnet ef migrations add "initial mig"
```

**Atualizando o Banco de Dados:**

Para aplicar as migrações ao banco de dados e atualizar seu esquema, execute:

```bash
dotnet ef database update
```

Este comando atualizará o banco de dados com todas as alterações feitas nas migrações. Você pode especificar o nome de uma migração específica ou usar um número para atualizar apenas as migrações até aquele ponto.

**Injeção de Dependência no Controller:**

No seu controller, injete a dependência do `DbContext` para acessar os dados:

```c#
private readonly AppDBContext _appDBContext;

public AtendimentoController(AppDBContext appDBContext)
{
    _appDBContext = appDBContext;
}
```

**Consultas e Manipulação de Dados:**

Com o `DbContext` injetado, você pode realizar consultas LINQ para recuperar dados do banco de dados e realizar operações CRUD (Create, Read, Update, Delete) em seus objetos.

**Recursos Adicionais:**

* Documentação oficial do Entity Framework: [https://learn.microsoft.com/en-us/ef/](https://learn.microsoft.com/en-us/ef/)
* Tutoriais e exemplos do EF: [https://learn.microsoft.com/en-us/ef/](https://learn.microsoft.com/en-us/ef/)
* Vídeos e cursos sobre o EF: 