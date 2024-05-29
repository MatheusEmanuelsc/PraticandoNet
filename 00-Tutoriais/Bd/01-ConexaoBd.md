Claro, vou trazer as informações de volta e corrigir o conteúdo para que esteja ajustado em Markdown com índice.

```markdown
# Tutorial de Projeto com Abordagem Code-First

## Índice
1. [Introdução](#introdução)
2. [Etapa 1: Criação dos Models](#etapa-1-criação-dos-models)
    1. [Criação dos Models](#criação-dos-models)
    2. [Exemplos de Entidades](#exemplos-de-entidades)
3. [Etapa 2: Configurando o Entity Framework (EF)](#etapa-2-configurando-o-entity-framework-ef)
    1. [Adicionando Pacotes](#etapa-21-adicionando-pacotes)
    2. [Adicionando a Ferramenta do EF](#etapa-22-adicionando-a-ferramenta-do-ef)
4. [Etapa 3: Criando a Classe de Contexto do EF](#etapa-3-criando-a-classe-de-contexto-do-ef)
5. [Etapa 4: Configurando a Conexão](#etapa-4-configurando-a-conexão)
    1. [String de Conexão](#etapa-41-string-de-conexão)
    2. [Passando as Informações de Conexão](#etapa-42-passando-as-informações-de-conexão)

## Introdução

Este tutorial tem como objetivo mostrar de forma linear o que foi feito neste projeto, facilitando o entendimento e oferecendo algumas sugestões. Seguiremos a abordagem Code-First.

## Etapa 1: Criação dos Models

### Criação dos Models

Após criar o projeto, primeiro você deve criar uma pasta chamada `Model` ou `Domain`, onde você criará suas entidades.

### Exemplos de Entidades

```csharp
namespace WebApplication1.Models
{
    public class Categoria
    {
        public int CategoriaId { get; set; }
        public string? Nome { get; set; }
        public string? ImgUrl { get; set; }
    }

    public class Produto
    {
        public int ProdutoId { get; set; }
        public string? Nome { get; set; }
        public string? Desc { get; set; }
        public decimal Preco { get; set; }
        public string? ImgUrl { get; set; }
        public double Estoque { get; set; }
        public DateTime DataCadastro { get; set; }
    }
}
```

Observe que os IDs têm o primeiro nome da entidade seguido de "Id". Isso acontece porque o Entity Framework (EF) os identifica dessa maneira. Você também pode deixar apenas "Id", mas caso use outra ordem, não funcionará.

Você também pode definir os relacionamentos aqui. Mais à frente, haverá um arquivo específico apenas para os relacionamentos entre entidades.

## Etapa 2: Configurando o Entity Framework (EF)

### Etapa 2.1: Adicionando Pacotes

#### Pacotes do EF

- `Pomelo.EntityFrameworkCore.MySql` (Vamos utilizar o MySQL)
- `Microsoft.EntityFrameworkCore.Design`

Para adicionar um pacote, utilize o comando:

```bash
dotnet add package NOME_PACOTE
```

### Etapa 2.2: Adicionando a Ferramenta do EF

Para instalar globalmente:

```bash
dotnet tool install -g dotnet-ef
```

Caso já tenha instalado, apenas atualize.

Opcionalmente, você pode conferir se seus pacotes estão instalados na pasta `WebApplication`.

## Etapa 3: Criando a Classe de Contexto do EF

Nesta classe definiremos os relacionamentos e as tabelas do projeto.

### Criando a Classe de Contexto

Crie uma pasta chamada `Context`, na qual você criará uma classe de contexto. Geralmente, você pode nomeá-la com o sufixo "Context", como no exemplo abaixo:

```csharp
namespace WebApplication1.Context
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Categoria>? Categorias { get; set; }
        public DbSet<Produto>? Produtos { get; set; }
    }
}
```

## Etapa 4: Configurando a Conexão

### Etapa 4.1: String de Conexão

A string de conexão é onde passamos nossos dados de acesso ao banco de dados. Cada banco de dados tem sua própria string de conexão.

Para configurar, vá até o arquivo `appsettings.json` e adicione:

```json
"ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=DbCatalogo;Uid=myUsername;Pwd=SUASENHA;"
}
```

### Etapa 4.2: Passando as Informações de Conexão

No arquivo `Program.cs`, adicione:

```csharp
var mysqlConnection = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseMySql(mysqlConnection, ServerVersion.AutoDetect(mysqlConnection));
});
```

Obs: Adicione este trecho de código antes do `app`.
```

Apenas aplicar as migrations agora.